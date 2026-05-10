using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class LowLevelPath
{
    public static readonly bool IsHostFileSystemCaseSensitive = 
        !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDirectorySeparator(char c)
    {
        return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
    }

    public static int GetCommonDirectoryPrefixLength(ReadOnlySpan<char> path, ReadOnlySpan<char> directory)
    {
        var minLen = Math.Min(path.Length, directory.Length);
        var lastSlash = 0;
        var caseSensitive = IsHostFileSystemCaseSensitive;
        int i;

        for (i = 0; i < minLen; i++)
        {
            var pc = path[i];
            var dc = directory[i];
            var pcIsSep = IsDirectorySeparator(pc);

            var charsEqual = pc == dc || (!caseSensitive && char.ToUpperInvariant(pc) == char.ToUpperInvariant(dc));

            if (!charsEqual && !(pcIsSep && IsDirectorySeparator(dc)))
                break;

            if (pcIsSep)
                lastSlash = i + 1;
        }

        if (i == minLen)
        {
            if (path.Length == directory.Length
                || (i == directory.Length && IsDirectorySeparator(path[i]))
                || (i == path.Length && IsDirectorySeparator(directory[i])))
                return i;
        }

        return lastSlash;
    }
}
