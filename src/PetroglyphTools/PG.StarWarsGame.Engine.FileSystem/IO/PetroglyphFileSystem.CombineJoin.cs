using System;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    /// <summary>
    /// Combines strings into a path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is intended to concatenate individual strings into a single string that represents a file path.
    /// However, if an argument other than the first contains a rooted path, any previous path components are ignored,
    /// and the returned string begins with that rooted path component.
    /// </para>
    /// <para>
    /// This method supports the directory separator characters ("/") and ("\").
    /// </para>
    /// </remarks>
    /// <param name="pathA">The first path to combine.</param>
    /// <param name="pathB">The second path to combine.</param>
    /// <returns>
    /// The combined paths. If one of the specified paths is a zero-length string, this method returns the other path.
    /// If <paramref name="pathB"/> contains an absolute path, this method returns <paramref name="pathB"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="pathA"/> or <paramref name="pathB"/> is <see langword="null"/>.
    /// </exception>
    public string CombinePath(string pathA, string pathB)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            return _underlyingFileSystem.Path.Combine(pathA, pathB);
        
        if (pathA == null)
            throw new ArgumentNullException(nameof(pathA));
        if (pathB == null)
            throw new ArgumentNullException(nameof(pathB));
        return CombineInternal(pathA, pathB);
    }
    
    internal void JoinPath(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ref ValueStringBuilder stringBuilder)
    {
        if (path1.Length == 0 && path2.Length == 0)
            return;

        if (path1.Length == 0 || path2.Length == 0)
        {
            ref var pathToUse = ref path1.Length == 0 ? ref path2 : ref path1;
            stringBuilder.Append(pathToUse);
            return;
        }

        stringBuilder.Append(path1);
        
        var hasSeparator = IsDirectorySeparator(path1[path1.Length - 1]) || IsDirectorySeparator(path2[0]);
        if (!hasSeparator)
            stringBuilder.Append(_underlyingFileSystem.Path.DirectorySeparatorChar);
        
        stringBuilder.Append(path2);
    }
    
    private string CombineInternal(string first, string second)
    {
        if (string.IsNullOrEmpty(first))
            return second;

        if (string.IsNullOrEmpty(second))
            return first;

        if (IsPathRooted(second.AsSpan()))
            return second;

        return JoinInternal(first, second);
    }

    private string JoinInternal(string first, string second)
    {
        var hasSeparator = IsDirectorySeparator(first[first.Length - 1]) || IsDirectorySeparator(second[0]);
        return hasSeparator
            ? string.Concat(first, second)
            : string.Concat(first, _underlyingFileSystem.Path.DirectorySeparatorChar, second);
    }
}