using System;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
#if NETSTANDARD2_1 || NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{

    /// <summary>
    /// The path string from which to obtain the file name and extension.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The characters after the last directory separator character in <paramref name="path"/>.
    /// If the last character of <paramref name="path"/> is a directory or volume separator character, this method returns Empty.
    /// If <paramref name="path"/> is <see langword="null"/>, this method returns <see langword="null"/>.</returns>
    /// <remarks>
    /// The returned value is <see langword="null"/> if the file path is <see langword="null"/>.
    /// The separator characters used to determine the start of the file name are ("/") and ("\").
    /// </remarks>
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

    /// <summary>
    /// Returns the file name and extension of a file path that is represented by a read-only character span.
    /// </summary>
    /// <param name="path">A read-only span that contains the path from which to obtain the file name and extension.</param>
    /// <returns>The characters after the last directory separator character in <paramref name="path"/>.</returns>
    /// <remarks>
    /// The returned read-only span contains the characters of the path that follow the last separator in path.
    /// If the last character in path is a volume or directory separator character, the method returns <see cref="ReadOnlySpan{T}.Empty"/>.
    /// If path contains no separator character, the method returns path.
    /// </remarks>
    public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            return _underlyingFileSystem.Path.GetFileName(path);
        
        var root = GetPathRoot(path).Length;
        var i = path.LastIndexOfAny(DirectorySeparatorChar, AltDirectorySeparatorChar);
        return path.Slice(i < root ? root : i + 1);
    }

    /// <summary>
    /// Returns the file name of the specified path string without the extension.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The string returned by <see cref="GetFileName(ReadOnlySpan{char})"/>, minus the last period (.) and all characters following it.</returns>
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

    /// <summary>
    /// Returns the file name without the extension of a file path that is represented by a read-only character span.
    /// </summary>
    /// <param name="path">A read-only span that contains the path from which to obtain the file name without the extension.</param>
    /// <returns>The characters in the read-only span returned by <see cref="GetFileName(ReadOnlySpan{char})"/>, minus the last period (.) and all characters following it.</returns>
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

    /// <summary>
    /// Changes the extension of a path string.
    /// </summary>
    /// <param name="path">The path information to modify.</param>
    /// <param name="extension">The new extension (with or without a leading period). Specify <see langword="null"/> to remove an existing extension from <paramref name="path"/>.</param>
    /// <returns>
    /// The modified path information.
    /// <para>
    /// If <paramref name="path"/> is <see langword="null"/> or an empty string (""), the path information is returned unmodified.
    /// If <paramref name="extension"/> is <see langword="null"/>, the returned string contains the specified path with its extension removed.
    /// If <paramref name="path"/> has no extension, and <paramref name="extension"/> is not <see langword="null"/>,
    /// the returned path string contains <paramref name="extension"/> appended to the end of <paramref name="path"/>.
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// If neither <paramref name="path"/> nor <paramref name="extension"/> contains a period (.), ChangeExtension adds the period.
    /// </para>
    /// <para>
    /// The <paramref name="extension"/> parameter can contain multiple periods and any valid path characters, and can be any length. If <paramref name="extension"/> is <see langword="null"/> ,
    /// the returned string contains the contents of <paramref name="path"/> with the last period and all characters following it removed.
    /// </para>
    /// <para>
    /// If <paramref name="extension"/> is an empty string, the returned path string contains the contents of <paramref name="path"/> with any characters following the last period removed.
    /// </para>
    /// <para>
    /// If <paramref name="path"/> does not have an extension and <paramref name="extension"/> is not <see langword="null"/>,
    /// the returned string contains <paramref name="path"/> followed by <paramref name="extension"/>.
    /// </para>
    /// <para>
    /// If <paramref name="extension"/> is not <see langword="null"/>  and does not contain a leading period, the period is added.
    /// </para>
    /// <para>
    /// If <paramref name="path"/> contains a multiple extension separated by multiple periods,
    /// the returned string contains the contents of <paramref name="path"/> with the last period and all characters following it replaced by <paramref name="extension"/>.
    /// For example, if <paramref name="path"/> is <c>"\Dir1\examples\pathtests.csx.txt"</c> and <paramref name="extension"/> is <c>"cs"</c>,
    /// the modified path is <c>"\Dir1\examples\pathtests.csx.cs"</c>.
    /// </para>
    /// <para>
    /// It is not possible to verify that the returned results are valid in all scenarios. For example,
    /// if <paramref name="path"/> is empty, <paramref name="extension"/> is appended.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Returns the directory information for the specified path represented by a character span.
    /// </summary>
    /// <param name="path">The path to retrieve the directory information from.</param>
    /// <returns>Directory information for <paramref name="path"/>, or an empty span if <paramref name="path"/> is <see langword="null"/>,
    /// an empty span, or a root (such as \, C:, or \server\share).</returns>
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