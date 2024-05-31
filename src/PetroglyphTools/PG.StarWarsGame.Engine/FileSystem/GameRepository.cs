using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Files.MEG.Files;
using PG.StarWarsGame.Files.MEG.Services;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.FileSystem;

// EaW file lookup works slightly different!
public class FocGameRepository : IGameRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSystem _fileSystem;
    private readonly PetroglyphDataEntryPathNormalizer _megPathNormalizer;
    private readonly ICrc32HashingService _crc32HashingService;
    private readonly IMegFileExtractor _megExtractor;
    private readonly IMegFileService _megFileService;
    private readonly ILogger? _logger;

    private readonly string _gameDirectory;

    private readonly IList<string> _modPaths = new List<string>();
    private readonly IList<string> _fallbackPaths = new List<string>();

    private readonly IVirtualMegArchive? _masterMegArchive;

    public GameEngineType EngineType => GameEngineType.Foc;

    public IRepository EffectsRepository { get; }
    
    public FocGameRepository(GameLocations gameLocations, IServiceProvider serviceProvider)
    {
        if (gameLocations == null)
            throw new ArgumentNullException(nameof(gameLocations));

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _megPathNormalizer = serviceProvider.GetRequiredService<PetroglyphDataEntryPathNormalizer>();
        _crc32HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _megExtractor = serviceProvider.GetRequiredService<IMegFileExtractor>();
        _megFileService = serviceProvider.GetRequiredService<IMegFileService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

        foreach (var mod in gameLocations.ModPaths)
        {
            if (string.IsNullOrEmpty(mod))
            {
                _logger?.LogTrace("Skipping null or empty mod path.");
                continue;
            }
            _modPaths.Add(_fileSystem.Path.GetFullPath(mod));
        }

        _gameDirectory = _fileSystem.Path.GetFullPath(gameLocations.GamePath);


        foreach (var fallbackPath in gameLocations.FallbackPaths)
        {
            if (string.IsNullOrEmpty(fallbackPath))
            {
                _logger?.LogTrace("Skipping null or empty fallback path.");
                continue;
            }
            _fallbackPaths.Add(_fileSystem.Path.GetFullPath(fallbackPath));
        }

        _masterMegArchive = CreateMasterMegArchive();

        EffectsRepository = new EffectsRepository(this, serviceProvider);
    }

    private IVirtualMegArchive CreateMasterMegArchive()
    {
        var builder = _serviceProvider.GetRequiredService<IVirtualMegArchiveBuilder>();

        var megsToConsider = new List<IMegFile>();

        // We assume that the first fallback path (if present at all) is always Empire at War.
        var firstFallback = _fallbackPaths.FirstOrDefault();
        if (firstFallback is not null)
        {
            var eawMegs = LoadMegArchivesFromXml(firstFallback);
            var eawPatch = LoadMegArchive(_fileSystem.Path.Combine(firstFallback, "Data\\Patch.meg"));
            var eawPatch2 = LoadMegArchive(_fileSystem.Path.Combine(firstFallback, "Data\\Patch2.meg"));
            var eaw64Patch = LoadMegArchive(_fileSystem.Path.Combine(firstFallback, "Data\\64Patch.meg"));

            megsToConsider.AddRange(eawMegs);
            if (eawPatch is not null)
                megsToConsider.Add(eawPatch);
            if (eawPatch2 is not null)
                megsToConsider.Add(eawPatch2);
            if (eaw64Patch is not null)
                megsToConsider.Add(eaw64Patch);
        }

        var focOrModMegs = LoadMegArchivesFromXml(".");
        var focPatch = LoadMegArchive("Data\\Patch.meg");
        var focPatch2 = LoadMegArchive("Data\\Patch2.meg");
        var foc64Patch = LoadMegArchive("Data\\64Patch.meg");

        megsToConsider.AddRange(focOrModMegs);
        if (focPatch is not null)
            megsToConsider.Add(focPatch);
        if (focPatch2 is not null)
            megsToConsider.Add(focPatch2);
        if (foc64Patch is not null)
            megsToConsider.Add(foc64Patch);

        return builder.BuildFrom(megsToConsider, true);
    }

    private IList<IMegFile> LoadMegArchivesFromXml(string lookupPath)
    {
        var megFilesXmlPath = _fileSystem.Path.Combine(lookupPath, "Data\\MegaFiles.xml");

        var fileParserFactory = _serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

        using var xmlStream = TryOpenFile(megFilesXmlPath);

        if (xmlStream is null)
        {
            _logger?.LogWarning($"Unable to find MegaFiles.xml at '{lookupPath}'");
            return Array.Empty<IMegFile>();
        }

        var parser = fileParserFactory.GetFileParser<XmlFileContainer>();
        var megaFilesXml = parser.ParseFile(xmlStream);


        var megs = new List<IMegFile>(megaFilesXml.Files.Count);

        foreach (var file in megaFilesXml.Files.Select(x => x.Trim()))
        {
            var megPath = _fileSystem.Path.Combine(lookupPath, file);
            var megFile = LoadMegArchive(megPath);
            if (megFile is not null)
                megs.Add(megFile);
        }

        return megs;
    }

    private IMegFile? LoadMegArchive(string megPath)
    {
        using var megFileStream = TryOpenFile(megPath);
        if (megFileStream is not FileSystemStream fileSystemStream)
        {
            _logger?.LogWarning($"Unable to find MEG data at '{megPath}'");
            return null;
        }

        var megFile = _megFileService.Load(fileSystemStream);

        if (megFile.FileInformation.FileVersion != MegFileVersion.V1)
            throw new InvalidOperationException("MEG data version must be V1.");

        return megFile;
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
            var newPath = _fileSystem.Path.ChangeExtension(filePath, extension);
            if (FileExists(newPath, megFileOnly))
                return true;
        }
        return false;
    }


    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        if (!megFileOnly)
        {
            // This is a custom rule
            if (_fileSystem.Path.IsPathFullyQualified(filePath))
                return _fileSystem.File.Exists(filePath);

            foreach (var modPath in _modPaths)
            {
                var modFilePath = _fileSystem.Path.Combine(modPath, filePath);
                if (_fileSystem.File.Exists(modFilePath))
                    return true;
            }

            var normalFilePath = _fileSystem.Path.Combine(_gameDirectory, filePath);
            if (_fileSystem.File.Exists(normalFilePath))
                return true;
        }

        if (_masterMegArchive is not null)
        {
            var normalizedPath = _megPathNormalizer.Normalize(filePath);
            var crc = _crc32HashingService.GetCrc32(normalizedPath, PGConstants.PGCrc32Encoding);

            var entry = _masterMegArchive.FirstEntryWithCrc(crc);
            if (entry is not null)
                return true;
        }

        if (!megFileOnly)
        {

            foreach (var fallbackPath in _fallbackPaths)
            {
                var fallbackFilePath = _fileSystem.Path.Combine(fallbackPath, filePath);
                if (_fileSystem.File.Exists(fallbackFilePath))
                    return true;
            }
        }

        return false;
    }

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        if (!megFileOnly)
        {
            // This is a custom rule
            if (_fileSystem.Path.IsPathFullyQualified(filePath))
                return !_fileSystem.File.Exists(filePath) ? null : OpenFileRead(filePath);

            foreach (var modPath in _modPaths)
            {
                var modFilePath = _fileSystem.Path.Combine(modPath, filePath);
                if (_fileSystem.File.Exists(modFilePath))
                    return OpenFileRead(modFilePath);
            }


            var normalFilePath = _fileSystem.Path.Combine(_gameDirectory, filePath);
            if (_fileSystem.File.Exists(normalFilePath))
                return OpenFileRead(normalFilePath);
        }

        if (_masterMegArchive is not null)
        {
            var normalizedPath = _megPathNormalizer.Normalize(filePath);
            var crc = _crc32HashingService.GetCrc32(normalizedPath, PGConstants.PGCrc32Encoding);

            var entry = _masterMegArchive.FirstEntryWithCrc(crc);
            if (entry is not null)
                return _megExtractor.GetFileData(entry.Location);
        }

        if (!megFileOnly)
        {
            foreach (var fallbackPath in _fallbackPaths)
            {
                var fallbackFilePath = _fileSystem.Path.Combine(fallbackPath, filePath);
                if (_fileSystem.File.Exists(fallbackFilePath))
                    return OpenFileRead(fallbackFilePath);
            }
        }
        return null;
    }

    private FileSystemStream OpenFileRead(string filePath)
    {
        if (!AllowOpenFile(filePath))
            throw new UnauthorizedAccessException("The data is not part of the Games!");
        return _fileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private bool AllowOpenFile(string filePath)
    {
        foreach (var modPath in _modPaths)
        {
            if (_fileSystem.Path.IsChildOf(modPath, filePath))
                return true;
        }

        if (_fileSystem.Path.IsChildOf(_gameDirectory, filePath))
            return true;

        foreach (var fallbackPath in _fallbackPaths)
        {
            if (_fileSystem.Path.IsChildOf(fallbackPath, filePath))
                return true;
        }

        return false;
    }
}