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

        var directory = _underlyingFileSystem.Path.GetDirectoryName(pathString);
        var fileName = _underlyingFileSystem.Path.GetFileName(pathString);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            return false;

        if (!_underlyingFileSystem.Directory.Exists(directory))
        {
            if (!FileExistsCaseInsensitive(directory.AsSpan(), ref stringBuilder))
                return false;

            directory = stringBuilder.AsSpan().ToString();
        }

        var files = _underlyingFileSystem.Directory.GetFiles(directory);
        var directories = _underlyingFileSystem.Directory.GetDirectories(directory);

        foreach (var file in files)
        {
            var name = _underlyingFileSystem.Path.GetFileName(file);
            if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(file);
                return true;
            }
        }

        foreach (var dir in directories)
        {
            var name = _underlyingFileSystem.Path.GetFileName(dir);
            if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(dir);
                return true;
            }
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