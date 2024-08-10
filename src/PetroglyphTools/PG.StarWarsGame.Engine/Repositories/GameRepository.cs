using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Files.MEG.Data.Entries;
using PG.StarWarsGame.Files.MEG.Data.EntryLocations;
using PG.StarWarsGame.Files.MEG.Files;
using PG.StarWarsGame.Files.MEG.Services;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Repositories;

internal abstract class GameRepository : ServiceBase, IGameRepository
{
    private readonly IMegFileService _megFileService;
    private readonly IMegFileExtractor _megExtractor;
    private readonly PetroglyphDataEntryPathNormalizer _megPathNormalizer;
    private readonly ICrc32HashingService _crc32HashingService;
    private readonly IVirtualMegArchiveBuilder _virtualMegBuilder;
    private readonly IGameLanguageManagerProvider _languageManagerProvider;
    private readonly DatabaseErrorListenerWrapper _errorListener;
    
    protected readonly string GameDirectory;

    protected readonly IList<string> ModPaths = new List<string>();
    protected readonly IList<string> FallbackPaths = new List<string>();

    private bool _sealed;

    public string Path { get; }
    public abstract GameEngineType EngineType { get; }

    public IRepository EffectsRepository { get; }
    public IRepository TextureRepository { get; }


    private readonly List<string> _loadedMegFiles = new();
    protected IVirtualMegArchive? MasterMegArchive { get; private set; }

    protected GameRepository(GameLocations gameLocations, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (gameLocations == null)
            throw new ArgumentNullException(nameof(gameLocations));

        _megExtractor = serviceProvider.GetRequiredService<IMegFileExtractor>();
        _megFileService = serviceProvider.GetRequiredService<IMegFileService>();
        _virtualMegBuilder = serviceProvider.GetRequiredService<IVirtualMegArchiveBuilder>();
        _crc32HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _megPathNormalizer = new PetroglyphDataEntryPathNormalizer(serviceProvider);
        _languageManagerProvider = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>();
        _errorListener = errorListener;

        foreach (var mod in gameLocations.ModPaths)
        {
            if (string.IsNullOrEmpty(mod))
                throw new InvalidOperationException("Mods with empty paths are not valid.");

            ModPaths.Add(FileSystem.Path.GetFullPath(mod));
        }

        GameDirectory = FileSystem.Path.GetFullPath(gameLocations.GamePath);


        foreach (var fallbackPath in gameLocations.FallbackPaths)
        {
            if (string.IsNullOrEmpty(fallbackPath))
            {
                Logger.LogTrace("Skipping null or empty fallback path.");
                continue;
            }
            FallbackPaths.Add(FileSystem.Path.GetFullPath(fallbackPath));
        }

        EffectsRepository = new EffectsRepository(this, serviceProvider);
        TextureRepository = new TextureRepository(this, serviceProvider);


        var path = ModPaths.Any() ? ModPaths.First() : GameDirectory;
        if (!FileSystem.Path.HasTrailingDirectorySeparator(path))
            path += FileSystem.Path.DirectorySeparatorChar;
        
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
            Logger.LogWarning($"Unable to find MEG file at '{megFile}'");
            return;
        }

        AddMegFiles([megArchive]);
    }

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        return RepositoryFileLookup(filePath, fp =>
            {
                if (FileSystem.File.Exists(fp))
                    return new ActionResult<bool>(true, true);
                return ActionResult<bool>.DoNotReturn;
            },
            fp =>
            {
                var entry = FindFileInMasterMeg(fp);
                if (entry is not null)
                    return new ActionResult<bool>(true, true);
                return ActionResult<bool>.DoNotReturn;
            }, megFileOnly);
    }

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        return RepositoryFileLookup(filePath, fp =>
            {
                if (FileSystem.File.Exists(fp))
                    return new ActionResult<Stream>(true, OpenFileRead(fp));
                return ActionResult<Stream>.DoNotReturn;
            },
            fp =>
            {
                var entry = FindFileInMasterMeg(fp);
                if (entry is not null)
                    return new ActionResult<Stream>(true, _megExtractor.GetFileData(entry.Location));
                return ActionResult<Stream>.DoNotReturn;
            }, megFileOnly);
    }


    public Stream OpenFile(string filePath, bool megFileOnly = false)
    {
        var stream = TryOpenFile(filePath, megFileOnly);
        if (stream is null)
            throw new FileNotFoundException($"Unable to find game data: {filePath}");
        return stream;
    }

    public bool FileExists(string filePath, string[] extensions, bool megFileOnly = false)
    {
        foreach (var extension in extensions)
        {
            var newPath = FileSystem.Path.ChangeExtension(filePath, extension);
            if (FileExists(newPath, megFileOnly))
                return true;
        }
        return false;
    }

    public IEnumerable<string> FindFiles(string searchPattern, bool megFileOnly = false)
    {
        var files = new HashSet<string>();

        var matcher = new Matcher();
        matcher.AddInclude(searchPattern);

        RepositoryFileLookup(searchPattern,
            pattern =>
            {
                var path = pattern.AsSpan().TrimEnd(searchPattern.AsSpan());

                var matcherResult = matcher.Execute(FileSystem, path.ToString());

                foreach (var matchedFile in matcherResult.Files)
                {
                    var normalizedFile = _megPathNormalizer.Normalize(matchedFile.Path);
                    files.Add(normalizedFile);
                }

                return ActionResult<EmptyStruct>.DoNotReturn;
            },
            _ =>
            {
                var foundFiles = MasterMegArchive!.FindAllEntries(searchPattern, true);
                foreach (var x in foundFiles)
                    files.Add(x.FilePath);

                return ActionResult<EmptyStruct>.DoNotReturn;
            }, megFileOnly);

        return files;
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
            var file = FileSystem.Path.GetFileName(loadedMegFile.AsSpan());
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
            var fallback2dNonLocalized = LoadMegArchive(FileSystem.Path.Combine(firstFallback, "DATA\\AUDIO\\SFX\\SFX2D_NON_LOCALIZED.MEG"));
            var fallback3dNonLocalized = LoadMegArchive(FileSystem.Path.Combine(firstFallback, "DATA\\AUDIO\\SFX\\SFX3D_NON_LOCALIZED.MEG"));

            if (fallback2dNonLocalized is not null)
                megsToAdd.Add(fallback2dNonLocalized);

            if (fallback3dNonLocalized is not null)
                megsToAdd.Add(fallback3dNonLocalized);
        }

        var nonLocalized2d = LoadMegArchive("DATA\\AUDIO\\SFX\\SFX2D_NON_LOCALIZED.MEG");
        var nonLocalized3d =  LoadMegArchive("DATA\\AUDIO\\SFX\\SFX3D_NON_LOCALIZED.MEG");

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
                var fallback2dLang = LoadMegArchive(FileSystem.Path.Combine(firstFallback, languageFiles.Sfx2dMegFilePath));
                if (fallback2dLang is not null)
                    megsToAdd.Add(fallback2dLang);
            }

            var lang2d = LoadMegArchive(languageFiles.Sfx2dMegFilePath);
            if (lang2d is not null)
                megsToAdd.Add(lang2d);
        }

        if (languages.Count == 0) 
            Logger.LogWarning("Unable to initialize any language.");

        AddMegFiles(megsToAdd);

        return languages;
    }

    protected IList<IMegFile> LoadMegArchivesFromXml(string lookupPath)
    {
        var megFilesXmlPath = FileSystem.Path.Combine(lookupPath, "Data\\MegaFiles.xml");

        var fileParserFactory = Services.GetRequiredService<IPetroglyphXmlFileParserFactory>();

        using var xmlStream = TryOpenFile(megFilesXmlPath);

        if (xmlStream is null)
        {
            Logger.LogWarning($"Unable to find MegaFiles.xml at '{lookupPath}'");
            return Array.Empty<IMegFile>();
        }

        var parser = fileParserFactory.GetFileParser<XmlFileContainer>(_errorListener);
        var megaFilesXml = parser.ParseFile(xmlStream);


        var megs = new List<IMegFile>(megaFilesXml.Files.Count);

        foreach (var file in megaFilesXml.Files.Select(x => x.Trim()))
        {
            var megPath = FileSystem.Path.Combine(lookupPath, file);
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

    protected abstract T? RepositoryFileLookup<T>(string filePath, Func<string, ActionResult<T>> pathAction,
        Func<string, ActionResult<T>> megAction, bool megFileOnly, T? defaultValue = default);

    protected IMegFile? LoadMegArchive(string megPath)
    {
        using var megFileStream = TryOpenFile(megPath);
        if (megFileStream is not FileSystemStream fileSystemStream)
            return null;

        var megFile = _megFileService.Load(fileSystemStream);

        if (megFile.FileInformation.FileVersion != MegFileVersion.V1)
            throw new InvalidOperationException("MEG data version must be V1.");

        return megFile;
    }

    protected MegDataEntryReference? FindFileInMasterMeg(string filePath)
    {
        Span<char> fileNameBuffer = stackalloc char[PGConstants.MaxPathLength];

        // TODO: Is the engine really "to-uppering" the input???
        var length = _megPathNormalizer.Normalize(filePath.AsSpan(), fileNameBuffer);
        var fileName = fileNameBuffer.Slice(0, length);
        var crc = _crc32HashingService.GetCrc32(fileName, PGConstants.PGCrc32Encoding);

        return MasterMegArchive?.FirstEntryWithCrc(crc);
    }

    protected FileSystemStream OpenFileRead(string filePath)
    {
        if (!AllowOpenFile(filePath))
            throw new UnauthorizedAccessException("The data is not part of the Games!");
        return FileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private bool AllowOpenFile(string filePath)
    {
        foreach (var modPath in ModPaths)
        {
            if (FileSystem.Path.IsChildOf(modPath, filePath))
                return true;
        }

        if (FileSystem.Path.IsChildOf(GameDirectory, filePath))
            return true;

        foreach (var fallbackPath in FallbackPaths)
        {
            if (FileSystem.Path.IsChildOf(fallbackPath, filePath))
                return true;
        }

        return false;
    }

    private void ThrowIfSealed()
    {
        if (_sealed)
            throw new InvalidOperationException("The object is sealed for modifications");
    }

    protected readonly struct ActionResult<T>(bool shallReturn, T? result)
    {
        public T? Result { get; } = result;

        public bool ShallReturn { get; } = shallReturn;

        public static ActionResult<T> DoNotReturn => default;
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct EmptyStruct;

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
            MasterTextDatFilePath = $"DATA\\TEXT\\MasterTextFile_{languageString}.DAT";
            Sfx2dMegFilePath = $"DATA\\AUDIO\\SFX\\SFX2D_{languageString}.MEG";
            SpeechMegFileName = $"{languageString}SPEECH.MEG";
        }
    }
}