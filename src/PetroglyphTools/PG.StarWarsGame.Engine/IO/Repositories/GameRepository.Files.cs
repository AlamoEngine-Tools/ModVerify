using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.MEG.Binary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal partial class GameRepository
{
    private static readonly string[] DataPathPrefixes = ["DATA/", "DATA\\", "./DATA/", ".\\DATA\\"];

    public bool FileExists(string filePath, string[] extensions, bool megFileOnly = false)
    {
        foreach (var extension in extensions)
        {
            var newPath = PGFileSystem.ChangeExtension(filePath, extension);
            if (FileExists(newPath, megFileOnly))
                return true;
        }
        return false;
    }

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        return FileExists(filePath.AsSpan(), megFileOnly);
    }

    public bool FileExists(string filePath, bool megFileOnly, out bool inMeg, [NotNullWhen(true)] out string? actualFilePath)
    {
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFound = FindFile(filePath, ref sb, megFileOnly);
        var fileExists = fileFound.FileFound;
        inMeg = fileFound.InMeg;
        if (!fileExists)
            actualFilePath = null;
        else 
            actualFilePath = fileFound.InMeg ? fileFound.MegDataEntryReference.Path : fileFound.FilePath.ToString();
        sb.Dispose();
        return fileExists;
    }

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        return FileExists(filePath, megFileOnly, out _);
    }

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly, out bool pathTooLong)
    {
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFound = FindFile(filePath, ref sb, megFileOnly);
        var fileExists = fileFound.FileFound;
        pathTooLong = fileFound.PathTooLong;
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
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFoundInfo = FindFile(filePath, ref sb, megFileOnly);
        var fileStream = OpenFileCore(fileFoundInfo);
        sb.Dispose();
        return fileStream;
    }
    
    /// <summary>
    /// The core routine for finding a file using the game's specific lookup rules.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="pathStringBuilder">The string builder used for constructing the file path.</param>
    /// <param name="megFileOnly">Whether to only search for files in MEG archives.</param>
    /// <returns>The file found information.</returns>
    protected internal abstract FileFoundInfo FindFile(ReadOnlySpan<char> filePath,
        ref ValueStringBuilder pathStringBuilder, bool megFileOnly = false);

    protected FileFoundInfo GetFileInfoFromMasterMeg(ReadOnlySpan<char> filePath)
    {
        Debug.Assert(MasterMegArchive is not null);

        var sb = new ValueStringBuilder(stackalloc char[Math.Max(filePath.Length, PGConstants.MaxMegEntryPathLength)]);
        sb.Append(filePath);
        PGFileSystem.NormalizePath(ref sb);

        if (sb.Length > PGConstants.MaxMegEntryPathLength)
        {
            _logger.LogWarning("Trying to open a MEG entry which is longer than 259 characters: '{FileName}'", sb.ToString());
            sb.Dispose();
            return default;
        }

        Span<char> fileNameSpan = stackalloc char[PGConstants.MaxMegEntryPathLength];

        if (!_megPathNormalizer.TryNormalize(sb.AsSpan(), fileNameSpan, out var length))
        {
            sb.Dispose();
            return default;
        }

        sb.Dispose();

        var fileName = fileNameSpan.Slice(0, length);

        var crc = _crc32HashingService.GetCrc32(fileName, MegFileConstants.MegDataEntryPathEncoding);

        var entry = MasterMegArchive!.EntriesWithCrc(crc).FirstOrDefault();

        return new FileFoundInfo(entry);
    }

    protected FileFoundInfo FindFileCore(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder)
    {
        var exists = PGFileSystem.FileExists(filePath, ref stringBuilder, GameDirectory.AsSpan());
        return !exists ? default : new FileFoundInfo(stringBuilder.AsSpan());
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

            PGFileSystem.JoinPath(fallbackPath.AsSpan(), pathWithNormalizedData, ref pathStringBuilder);
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
            return _megExtractor.GetData(fileFoundInfo.MegDataEntryReference.Location);

        return PGFileSystem.OpenRead(fileFoundInfo.FilePath.ToString());
    }
}
