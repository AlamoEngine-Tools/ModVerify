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
    
    protected readonly string GameDirectory;

    protected readonly IList<string> ModPaths = new List<string>();
    protected readonly IList<string> FallbackPaths = new List<string>();

    private bool _sealed;

    public abstract GameEngineType EngineType { get; }

    public IRepository EffectsRepository { get; }
    
    protected IVirtualMegArchive? MasterMegArchive { get; private set; }

    protected GameRepository(GameLocations gameLocations, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (gameLocations == null)
            throw new ArgumentNullException(nameof(gameLocations));

        _megExtractor = serviceProvider.GetRequiredService<IMegFileExtractor>();
        _megFileService = serviceProvider.GetRequiredService<IMegFileService>();
        _virtualMegBuilder = serviceProvider.GetRequiredService<IVirtualMegArchiveBuilder>();
        _crc32HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _megPathNormalizer = new PetroglyphDataEntryPathNormalizer(serviceProvider);

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

        var parser = fileParserFactory.GetFileParser<XmlFileContainer>();
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
        // TODO To Span, as we don't use the name elsewhere
        var normalizedPath = _megPathNormalizer.Normalize(filePath);
        var crc = _crc32HashingService.GetCrc32(normalizedPath, PGConstants.PGCrc32Encoding);

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
}