using System;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    /// <summary>
    /// Determines whether two file system paths are considered equal.
    /// </summary>
    /// <param name="pathA">The first path to compare.</param>
    /// <param name="pathB">The second path to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the paths are considered equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool PathsAreEqual(string pathA, string pathB)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return _underlyingFileSystem.Path.AreEqual(pathA, pathB);
        
        var normalizedA = PathNormalizer.Normalize(pathA, PGFileSystemDirectorySeparatorNormalizeOptions);
        var normalizedB = PathNormalizer.Normalize(pathB, PGFileSystemDirectorySeparatorNormalizeOptions);

        var fullA = _underlyingFileSystem.Path.GetFullPath(normalizedA);
        var fullB = _underlyingFileSystem.Path.GetFullPath(normalizedB);
        
        return PathsEqual(fullA.AsSpan(), fullB.AsSpan(), Math.Max(fullA.Length, fullB.Length));
    }
    
    private static bool PathsEqual(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, int length)
    {
        if (path1.Length < length || path2.Length < length)
            return false;
        
        for (var i = 0; i < length; i++)
        {
            if (!PathCharEqual(path1[i], path2[i]))
                return false;
        }
        return true;
    }

    private static bool PathCharEqual(char x, char y)
    {
        if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
            return true;
        return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
    }
}