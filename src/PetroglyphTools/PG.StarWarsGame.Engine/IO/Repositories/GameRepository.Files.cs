using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.IO.Utilities;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.MEG.Binary;

namespace PG.StarWarsGame.Engine.IO.Repositories;

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
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFound = FindFile(filePath, ref sb, megFileOnly);
        var fileExists = fileFound.FileFound;
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

    protected internal abstract FileFoundInfo FindFile(ReadOnlySpan<char> filePath,
        ref ValueStringBuilder pathStringBuilder, bool megFileOnly = false);

    protected FileFoundInfo GetFileInfoFromMasterMeg(ReadOnlySpan<char> filePath)
    {
        Debug.Assert(MasterMegArchive is not null);

        if (filePath.Length > PGConstants.MaxMegEntryPathLength)
        {
            Logger.LogWarning($"Trying to open a MEG entry which is longer than 259 characters: '{filePath.ToString()}'");
            return default;
        }

        Span<char> fileNameSpan = stackalloc char[PGConstants.MaxMegEntryPathLength];

        if (!_megPathNormalizer.TryNormalize(filePath, fileNameSpan, out var length))
            return default;

        var fileName = fileNameSpan.Slice(0, length);

        if (fileName.Length > PGConstants.MaxMegEntryPathLength)
        {
            Logger.LogWarning($"Trying to open a MEG entry which is longer than 259 characters after normalization: '{fileName.ToString()}'");
            return default;
        }

        var crc = _crc32HashingService.GetCrc32(fileName, MegFileConstants.MegDataEntryPathEncoding);

        var entry = MasterMegArchive!.EntriesWithCrc(crc).FirstOrDefault();

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

        // We accept a *possible* difference here between platforms,
        // unless it's proven the differences are too significant.
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            exists = FileSystem.File.Exists(actualFilePath.ToString());
        else
        {
            // We *could* also use the slightly faster GetFileAttributesA.
            // However, CreateFileA and GetFileAttributesA are implemented complete independent.
            // The game uses CreateFileA.
            // Thus, we should stick to what the game uses in order to be as close to the engine as possible
            // NB: It's also important that the string builder is zero-terminated, as otherwise CreateFileA might get invalid data.
            var fileHandle = CreateFile(
                in stringBuilder.GetPinnableReference(true),
                FileAccess.Read,
                FileShare.Read,
                IntPtr.Zero,
                FileMode.Open,
                FileAttributes.Normal, IntPtr.Zero);

            exists = IsValidAndClose(fileHandle);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidAndClose(IntPtr handle)
    {
        var isValid = handle != IntPtr.Zero && handle != new IntPtr(-1);
        if (isValid)
            CloseHandle(handle);
        return isValid;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        in char lpFileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess access,
        [MarshalAs(UnmanagedType.U4)] FileShare share,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);
}
