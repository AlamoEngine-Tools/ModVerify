using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;
#if NETSTANDARD2_1 || NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    internal bool FileExists(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder stringBuilder,
        ReadOnlySpan<char> gameDirectory)
    {
        stringBuilder.Length = 0;

        if (IsPathFullyQualified_Exists(filePath))
            stringBuilder.Append(filePath);
        else
            JoinPath(gameDirectory, filePath, ref stringBuilder);
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            NormalizePath(ref stringBuilder);
            
            var actualFilePath = stringBuilder.AsSpan();
            return FileExistsCaseInsensitive(actualFilePath, ref stringBuilder);
        }

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
            
        return IsValidAndClose(fileHandle);
    }
    
    // NB: This method assumes backslashes have been normalized to forward slashes
    // NB: This method operates on the actual file system
    private bool FileExistsCaseInsensitive(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder)
{
    Debug.Assert(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

    var pathString = filePath.ToString();
    if (_underlyingFileSystem.File.Exists(pathString))
        return true;

    var segments = pathString.Split('/');
    var currentPath = segments[0].Length == 0 ? "/" : segments[0];

    var lastSegmentIndex = segments.Length - 1;
    while (lastSegmentIndex > 0 && string.IsNullOrEmpty(segments[lastSegmentIndex]))
        lastSegmentIndex--;

    for (var i = 1; i < segments.Length; i++)
    {
        var segment = segments[i];
        if (string.IsNullOrEmpty(segment))
            continue;

        var isLastSegment = i == lastSegmentIndex;

        // Guard: if currentPath doesn't exist as a directory, bail out cheaply
        if (!_underlyingFileSystem.Directory.Exists(currentPath))
            return false;

        if (isLastSegment)
        {
            // Single IO call instead of GetFiles + GetDirectories
            var entries = _underlyingFileSystem.Directory.GetFileSystemEntries(currentPath);
            foreach (var entry in entries)
            {
                var name = _underlyingFileSystem.Path.GetFileName(entry);
                if (name.Equals(segment, StringComparison.OrdinalIgnoreCase))
                {
                    stringBuilder.Length = 0;
                    stringBuilder.Append(entry);
                    return true;
                }
            }
            return false;
        }

        // Intermediate segment: resolve case-insensitively
        var subDirs = _underlyingFileSystem.Directory.GetDirectories(currentPath);
        string? resolved = null;
        foreach (var dir in subDirs)
        {
            var name = _underlyingFileSystem.Path.GetFileName(dir);
            if (name.Equals(segment, StringComparison.OrdinalIgnoreCase))
            {
                resolved = dir;
                break;
            }
        }

        if (resolved is null)
            return false;

        currentPath = resolved;
    }

    return false;
}

    private bool IsPathFullyQualified_Exists(ReadOnlySpan<char> path)
    {
        // This is really tricky, because under Windows "/" or "\" do NOT
        // indicate a fully qualified path, under Linux however "/" does. 
        // The PGFileSystem is implemented to treat backslashes as directory separators.
        // However, this must not happen here, since we are operating on the actual file system.
        // E.g, \\Data\\Art\\... MUST not be treated as a fully qualified path
        // This means, ultimately, we can just delegate to the underlying file system.

        return _underlyingFileSystem.Path.IsPathFullyQualified(path);
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