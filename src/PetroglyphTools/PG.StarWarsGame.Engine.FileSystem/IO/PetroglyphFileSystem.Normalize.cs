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
        stringBuilder.Length = NormalizePath(stringBuilder.RawChars.Slice(0, stringBuilder.Length));
    }
    
    // TODO: Check whether we can eliminate the double slash normalization
    // once we migrated to PGFileSystem
    private static int NormalizePath(Span<char> path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return path.Length;

        var writePos = 0;
        var lastWasSeparator = false;
        for (var i = 0; i < path.Length; i++)
        {
            var c = path[i];
            var isSeparator = c is '\\' or '/';
            if (isSeparator && lastWasSeparator)
                continue;
            path[writePos++] = isSeparator ? '/' : c;
            lastWasSeparator = isSeparator;
        }

        return writePos;
    } 
}