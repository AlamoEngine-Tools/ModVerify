using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    internal void NormalizePath(ref ValueStringBuilder stringBuilder)
    {
        NormalizePath(stringBuilder.RawChars.Slice(0, stringBuilder.Length));
    }
    
    // TODO: Check whether we can eliminate the double slash normalization
    // once we migrated to PGFileSystem
    private static void NormalizePath(Span<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        for (var i = 0; i < path.Length; i++)
        {
            var c = path[i];
            if (IsDirectorySeparator(c))
                path[i] = '/';
        }
    } 
}