using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Files.MEG.Data.Entries;
using PG.StarWarsGame.Files.MEG.Data.EntryLocations;
using PG.StarWarsGame.Files.MEG.Files;
using PG.StarWarsGame.Files.MEG.Services;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal abstract partial class GameRepository : IGameRepository
{
    private readonly IMegFileService _megFileService;
    private readonly IMegFileExtractor _megExtractor;
    private readonly PetroglyphMegDataEntryPathNormalizer _megPathNormalizer;
    private readonly ICrc32HashingService _crc32HashingService;
    private readonly IVirtualMegArchiveBuilder _virtualMegBuilder;
    private readonly IGameLanguageManagerProvider _languageManagerProvider;
    private readonly GameEngineErrorReporterWrapper _errorReporter;

    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    
    protected readonly string GameDirectory;

    protected readonly IList<string> ModPaths = new List<string>();
    protected readonly IList<string> FallbackPaths = new List<string>();

    private bool _sealed;
    
    public PetroglyphFileSystem PGFileSystem { get; }

    public string Path { get; }
    
    public abstract GameEngineType EngineType { get; }

    public IRepository EffectsRepository { get; }
    public IRepository TextureRepository { get; }
    public IRepository ModelRepository { get; }
    

    private readonly List<string> _loadedMegFiles = new();
    protected IVirtualMegArchive? MasterMegArchive { get; private set; }

    protected GameRepository(
        GameLocations gameLocations,
        GameEngineErrorReporterWrapper errorReporter,
        IServiceProvider serviceProvider)
    {
        if (gameLocations == null)
            throw new ArgumentNullException(nameof(gameLocations));

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;

        _megExtractor = serviceProvider.GetRequiredService<IMegFileExtractor>();
        _megFileService = serviceProvider.GetRequiredService<IMegFileService>();
        _virtualMegBuilder = serviceProvider.GetRequiredService<IVirtualMegArchiveBuilder>();
        _crc32HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _megPathNormalizer = new EmpireAtWarMegDataEntryPathNormalizer();
        _languageManagerProvider = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>();
        _errorReporter = errorReporter;

        PGFileSystem = new PetroglyphFileSystem(serviceProvider);

        foreach (var mod in gameLocations.ModPaths)
        {
            if (string.IsNullOrEmpty(mod))
                throw new InvalidOperationException("Mods with empty paths are not valid.");

            ModPaths.Add(PGFileSystem.UnderlyingFileSystem.Path.GetFullPath(mod));
        }

        // NB: We are using the native file system here, because we want to make sure that
        // the paths are normalized to the actual file system of the current system.

        GameDirectory = PGFileSystem.UnderlyingFileSystem.Path.GetFullPath(gameLocations.GamePath);

        foreach (var fallbackPath in gameLocations.FallbackPaths)
        {
            if (string.IsNullOrEmpty(fallbackPath))
            {
                _logger.LogTrace("Skipping null or empty fallback path.");
                continue;
            }

            FallbackPaths.Add(PGFileSystem.UnderlyingFileSystem.Path.GetFullPath(fallbackPath));
        }

        EffectsRepository = new EffectsRepository(this);
        TextureRepository = new TextureRepository(this);
        ModelRepository = new ModelRepository(this);

        var path = ModPaths.Any() ? ModPaths.First() : GameDirectory;
        if (!PGFileSystem.UnderlyingFileSystem.Path.HasTrailingDirectorySeparator(path))
            path += PGFileSystem.UnderlyingFileSystem.Path.DirectorySeparatorChar;

        Path = path;
    }


    public void AddMegFiles(IList<IMegFile> megFiles)
    {
        ThrowIfSealed();
        if (MasterMegArchive is null)
            MasterMegArchive = _virtualMegBuilder.BuildFrom(megFiles, true);
        else
        {
            var newLocations = new List<MegDataEntryReference>();
            foreach (var megFile in megFiles)
                newLocations.AddRange(megFile.Archive.Select(entry =>
                    new MegDataEntryReference(new MegDataEntryLocationReference(megFile, entry))));
            MasterMegArchive = _virtualMegBuilder.BuildFrom(MasterMegArchive.ToList().Concat(newLocations), true);
        }

        _loadedMegFiles.AddRange(megFiles.Select(x => x.FilePath));
    }

    public void AddMegFile(string megFile)
    {
        var megArchive = LoadMegArchive(megFile);
        if (megArchive is null)
        {
            if (IsSpeechMeg(megFile))
                _logger.LogDebug("Unable to find Speech MEG file at '{MegFile}'", megFile);
            else
                _logger.LogWarning("Unable to find MEG file at '{MegFile}'", megFile);
            return;
        }

        AddMegFiles([megArchive]);
    }


    public bool IsLanguageInstalled(LanguageType language)
    {
        // A language is considered to be installed if its Text, Speech and localized 2d file exists in the current game
        var languageFiles = new LanguageFiles(language);

        if (!FileExists(languageFiles.MasterTextDatFilePath))
            return false;

        if (!FileExists(languageFiles.Sfx2dMegFilePath))
            return false;


        foreach (var loadedMegFile in _loadedMegFiles)
        {
            var file = PGFileSystem.UnderlyingFileSystem.Path.GetFileName(loadedMegFile.AsSpan());
            var speechFileName = languageFiles.SpeechMegFileName.AsSpan();

            if (file.Equals(speechFileName, StringComparison.OrdinalIgnoreCase)) 
                return true;
        }

        return false;
    }

    public IEnumerable<LanguageType> InitializeInstalledSfxMegFiles()
    {
        ThrowIfSealed();

        var megsToAdd = new List<IMegFile>();

        var firstFallback = FallbackPaths.FirstOrDefault();
        if (firstFallback is not null)
        {
            var fallback2dNonLocalized = LoadMegArchive(PGFileSystem.UnderlyingFileSystem.Path
                .Combine(firstFallback, "DATA/AUDIO/SFX/SFX2D_NON_LOCALIZED.MEG"));
            var fallback3dNonLocalized = LoadMegArchive(PGFileSystem.UnderlyingFileSystem.Path
                .Combine(firstFallback, "DATA/AUDIO/SFX/SFX3D_NON_LOCALIZED.MEG"));

            if (fallback2dNonLocalized is not null)
                megsToAdd.Add(fallback2dNonLocalized);

            if (fallback3dNonLocalized is not null)
                megsToAdd.Add(fallback3dNonLocalized);
        }

        var nonLocalized2d = LoadMegArchive("DATA/AUDIO/SFX/SFX2D_NON_LOCALIZED.MEG");
        var nonLocalized3d =  LoadMegArchive("DATA/AUDIO/SFX/SFX3D_NON_LOCALIZED.MEG");

        if (nonLocalized2d is not null)
            megsToAdd.Add(nonLocalized2d);

        if (nonLocalized3d is not null)
            megsToAdd.Add(nonLocalized3d);


        var languageManager = _languageManagerProvider.GetLanguageManager(EngineType);
        var languages = new List<LanguageType>();
        foreach (var language in languageManager.SupportedLanguages)
        {
            if (!IsLanguageInstalled(language))
                continue;
            languages.Add(language);

            var languageFiles = new LanguageFiles(language);

            if (firstFallback is not null)
            {
                var fallback2dLang = LoadMegArchive(PGFileSystem.UnderlyingFileSystem.Path
                    .Combine(firstFallback, languageFiles.Sfx2dMegFilePath));
                if (fallback2dLang is not null)
                    megsToAdd.Add(fallback2dLang);
            }

            var lang2d = LoadMegArchive(languageFiles.Sfx2dMegFilePath);
            if (lang2d is not null)
                megsToAdd.Add(lang2d);
        }

        if (languages.Count == 0) 
            _logger.LogWarning("Unable to initialize any language.");

        AddMegFiles(megsToAdd);

        return languages;
    }

    protected IList<IMegFile> LoadMegArchivesFromXml(string lookupPath)
    {
        var megFilesXmlPath = PGFileSystem.CombinePath(lookupPath, "Data/MegaFiles.xml");

        using var xmlStream = TryOpenFile(megFilesXmlPath);

        if (xmlStream is null)
        {
            _logger.LogWarning("Unable to find MegaFiles.xml at '{LookupPath}'", lookupPath);
            return Array.Empty<IMegFile>();
        }

        var parser = new XmlFileListParser(_serviceProvider ,_errorReporter);
        var megaFilesXml = parser.ParseFile(xmlStream);

        if (megaFilesXml is null)
            return [];

        var megs = new List<IMegFile>(megaFilesXml.Files.Count);

        foreach (var file in megaFilesXml.Files.Select(x => x.Trim()))
        {
            var megPath = PGFileSystem.CombinePath(lookupPath, file);
            var megFile = LoadMegArchive(megPath);
            if (megFile is not null)
                megs.Add(megFile);
        }

        return megs;
    }

    internal void Seal()
    {
        _sealed = true;
    }

    protected IMegFile? LoadMegArchive(string megPath)
    {
        using var megFileStream = TryOpenFile(megPath);
        if (megFileStream is not FileSystemStream fileSystemStream)
        {
            if (IsSpeechMeg(megPath))
                _logger.LogDebug("Unable to find Speech MEG file '{MegPath}'", megPath);
            else
            {
                var message = $"Unable to find MEG file '{megPath}'";
                _errorReporter.Assert(EngineAssert.Create(EngineAssertKind.FileNotFound, megPath, [], message));
                _logger.LogWarning("Unable to find MEG file '{MegPath}'", megPath);
            }
            return null;
        }

        var megFile = _megFileService.Load(fileSystemStream);

        if (megFile.FileInformation.FileVersion != MegFileVersion.V1)
            throw new InvalidOperationException("MEG data version must be V1.");

        return megFile;
    }

    private bool IsSpeechMeg(string megFile)
    {
        return PGFileSystem.GetFileName(megFile.AsSpan()).EndsWith("Speech.meg".AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    private void ThrowIfSealed()
    {
        if (_sealed)
            throw new InvalidOperationException("The object is sealed for modifications");
    }
    
    private sealed class LanguageFiles
    {
        public LanguageType Language { get; }

        public string MasterTextDatFilePath { get; }

        public string Sfx2dMegFilePath { get; }

        public string SpeechMegFileName { get; }

        public LanguageFiles(LanguageType language)
        {
            Language = language;
            var languageString = language.ToString().ToUpperInvariant();
            MasterTextDatFilePath = $"DATA/TEXT/MasterTextFile_{languageString}.DAT";
            Sfx2dMegFilePath = $"DATA/AUDIO/SFX/SFX2D_{languageString}.MEG";
            SpeechMegFileName = $"{languageString}SPEECH.MEG";
        }
    }
}