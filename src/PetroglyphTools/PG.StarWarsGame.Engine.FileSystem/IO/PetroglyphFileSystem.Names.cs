using System;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
#if NETSTANDARD2_1 || NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    
#if NETSTANDARD2_1 || NET
    [return: NotNullIfNotNull(nameof(path))]
#endif
    public string? GetFileName(string? path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.GetFileName(path);
        
        if (path == null)
            return null;
        var result = GetFileName(path.AsSpan());
        if (path.Length == result.Length)
            return path;
        return result.ToString();
    }

    public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            return _underlyingFileSystem.Path.GetFileName(path);
        
        var root = GetPathRoot(path).Length;
        var i = path.LastIndexOfAny(DirectorySeparatorChar, AltDirectorySeparatorChar);
        return path.Slice(i < root ? root : i + 1);
    }

#if NETSTANDARD2_1 || NET
    [return: NotNullIfNotNull(nameof(path))]
#endif
    public string? GetFileNameWithoutExtension(string? path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.GetFileNameWithoutExtension(path);

        if (path == null)
            return null;

        var result = GetFileNameWithoutExtension(path.AsSpan());
        return path.Length == result.Length
            ? path
            : result.ToString();
    }

    public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.GetFileNameWithoutExtension(path);
        var fileName = GetFileName(path);
        var lastPeriod = fileName.LastIndexOf('.');
        return lastPeriod < 0
            ? fileName
            : // No extension was found
            fileName.Slice(0, lastPeriod);
    }

#if NETSTANDARD2_1 || NET
    [return: NotNullIfNotNull(nameof(path))]
#endif
    public string? ChangeExtension(string? path, string? extension)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.ChangeExtension(path, extension);

        if (path == null)
            return null;

        var subLength = path.Length;
        if (subLength == 0)
            return string.Empty;

        for (var i = path.Length - 1; i >= 0; i--)
        {
            var ch = path[i];

            if (ch == '.')
            {
                subLength = i;
                break;
            }

            if (IsDirectorySeparator(ch))
                break;
        }

        if (extension == null)
            return path.Substring(0, subLength);

#if NETCOREAPP3_0_OR_GREATER
        var subpath = path.AsSpan(0, subLength);
        return extension.StartsWith('.') ?
            string.Concat(subpath, extension) :
            string.Concat(subpath, ".", extension);
#else
        var subPath = path.Substring(0, subLength);
        if (extension.Length >= 1 && extension[0] == '.')
            return string.Concat(subPath, extension);
        return string.Concat(subPath, ".", extension);
#endif
    }
    
    public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.GetDirectoryName(path);
        
        if (IsEffectivelyEmpty(path))
            return ReadOnlySpan<char>.Empty;

        var end = GetDirectoryNameOffset(path);
        return end >= 0 ? path.Slice(0, end) : ReadOnlySpan<char>.Empty;
    }
    
    private static int GetDirectoryNameOffset(ReadOnlySpan<char> path)
    {
        var rootLength = GetRootLength(path);
        var end = path.Length;
        if (end <= rootLength)
            return -1;

        while (end > rootLength && !IsDirectorySeparator(path[--end])) ;

        // Trim off any remaining separators (to deal with C:\foo\\bar)
        while (end > rootLength && IsDirectorySeparator(path[end - 1]))
            end--;

        return end;
    }
}