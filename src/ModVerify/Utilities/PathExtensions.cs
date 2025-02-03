using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;

namespace AET.ModVerify.Utilities;

public static class PathExtensions
{
    public static ReadOnlySpan<char> GetGameStrippedPath(this IPath path, ReadOnlySpan<char> gamePath, ReadOnlySpan<char> modPath)
    {
        if (!path.IsPathFullyQualified(modPath))
            return modPath;

        if (modPath.Length <= gamePath.Length)
            return modPath;

        if (path.IsChildOf(gamePath, modPath))
            return modPath.Slice(gamePath.Length);

        return modPath;
    }
}