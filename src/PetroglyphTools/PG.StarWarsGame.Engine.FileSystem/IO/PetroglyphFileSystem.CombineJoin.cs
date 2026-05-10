using System;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
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