using System;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    internal void NormalizePath(ref ValueStringBuilder stringBuilder)
    {
        NormalizePath(stringBuilder.RawChars.Slice(0, stringBuilder.Length));
    }
    
    private static void NormalizePath(Span<char> path)
    {
        PathNormalizer.Normalize(path, path, LinuxDirectorySeparatorNormalizeOptions);
        
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    return;
        //for (var i = 0; i < path.Length; i++)
        //{
        //    var c = path[i];
        //    if (IsDirectorySeparator(c))
        //        path[i] = '/';
        //}
    } 
}