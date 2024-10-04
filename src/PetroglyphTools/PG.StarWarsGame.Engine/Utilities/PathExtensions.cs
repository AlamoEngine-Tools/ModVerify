using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.Commons.Utilities;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class PathExtensions
{
    public static void Join(this IPath _, ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ref ValueStringBuilder stringBuilder)
    {
        if (path1.Length == 0 && path2.Length == 0)
            return;

        if (path1.Length == 0 || path2.Length == 0)
        {
            ref var pathToUse = ref path1.Length == 0 ? ref path2 : ref path1;
            stringBuilder.Append(pathToUse);
            return;
        }

        var needsSeparator = !(_.HasTrailingDirectorySeparator(path1) || _.HasLeadingDirectorySeparator(path2));

        stringBuilder.Append(path1);
        if (needsSeparator)
            stringBuilder.Append(_.DirectorySeparatorChar);

        stringBuilder.Append(path2);
    }
}