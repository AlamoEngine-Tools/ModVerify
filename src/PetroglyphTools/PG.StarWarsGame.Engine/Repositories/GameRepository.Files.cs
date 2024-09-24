using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.Repositories;

internal partial class GameRepository
{
    private static readonly string[] DataPathPrefixes = ["DATA/", "DATA\\", "./DATA/", ".\\DATA\\"];

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

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        return FileExists(filePath.AsSpan(), megFileOnly);
    }

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fileExists = FindFile(filePath, ref sb, megFileOnly).FileFound;
        sb.Dispose();
        return fileExists;
    }

    public Stream OpenFile(string filePath, bool megFileOnly = false)
    {
        return OpenFile(filePath.AsSpan(), megFileOnly);
    }


    public Stream OpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var stream = TryOpenFile(filePath, megFileOnly);
        if (stream is null)
            throw new FileNotFoundException($"Unable to find game data: {filePath.ToString()}");
        return stream;
    }

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        return TryOpenFile(filePath.AsSpan(), megFileOnly);
    }

    public Stream? TryOpenFile(ReadOnlySpan<char> filePath, bool megFileOnly)
    {
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fileFoundInfo = FindFile(filePath, ref sb, megFileOnly);
        var fileStream = OpenFileCore(fileFoundInfo);
        sb.Dispose();
        return fileStream;
    }

    protected internal abstract FileFoundInfo FindFile(ReadOnlySpan<char> filePath,
        ref ValueStringBuilder pathStringBuilder, bool megFileOnly = false);

    protected FileFoundInfo GetFileInfoFromMasterMeg(ReadOnlySpan<char> filePath)
    {
        Debug.Assert(MasterMegArchive is not null);

        Span<char> fileNameBuffer = stackalloc char[PGConstants.MaxPathLength];

        var length = _megPathNormalizer.Normalize(filePath, fileNameBuffer);
        var fileName = fileNameBuffer.Slice(0, length);
        var crc = _crc32HashingService.GetCrc32(fileName, PGConstants.PGCrc32Encoding);

        var entry = MasterMegArchive!.FirstEntryWithCrc(crc);

        return new FileFoundInfo(entry);
    }

    protected FileFoundInfo FindFileCore(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder)
    {
        bool exists;


        stringBuilder.Length = 0;

        if (FileSystem.Path.IsPathFullyQualified(filePath))
            stringBuilder.Append(filePath);
        else
            FileSystem.Path.Join(GameDirectory.AsSpan(), filePath, ref stringBuilder);

        var actualFilePath = stringBuilder.AsSpan();


        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            exists = FileSystem.File.Exists(actualFilePath.ToString());
        else
        {
            // The game uses CreateFileA which we can also use directly with the string span.
            exists = FileSystem.File.Exists(actualFilePath.ToString());
        }
        return !exists ? new FileFoundInfo() : new FileFoundInfo(actualFilePath);
    }

    protected FileFoundInfo FileFromAltExists(ReadOnlySpan<char> filePath, IList<string> fallbackPaths, ref ValueStringBuilder pathStringBuilder)
    {
        if (fallbackPaths.Count == 0)
            return default;
        if (!PathStartsWithDataDirectory(filePath, out var prefixLength))
            return default;

        var pathWithNormalizedData = filePath.Slice(prefixLength);

        foreach (var fallbackPath in fallbackPaths)
        {
            pathStringBuilder.Length = 0;

            FileSystem.Path.Join(fallbackPath.AsSpan(), pathWithNormalizedData, ref pathStringBuilder);
            var newPath = pathStringBuilder.AsSpan();

            var fileFoundInfo = FindFileCore(newPath, ref pathStringBuilder);
            if (fileFoundInfo.FileFound)
                return fileFoundInfo;
        }

        return default;
    }

    private static bool PathStartsWithDataDirectory(ReadOnlySpan<char> path, out int cutoffLength)
    {
        cutoffLength = 0;
        if (path.Length < 5)
            return false;
        foreach (var prefix in DataPathPrefixes)
        {
            if (path.StartsWith(prefix.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                if (path[0] == '.')
                    cutoffLength = 2;
                return true;
            }
        }
        return false;
    }

    internal Stream? OpenFileCore(FileFoundInfo fileFoundInfo)
    {
        if (!fileFoundInfo.FileFound)
            return null;

        if (fileFoundInfo.InMeg)
            return _megExtractor.GetFileData(fileFoundInfo.MegDataEntryReference.Location);

        return FileSystem.FileStream.New(fileFoundInfo.FilePath.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
