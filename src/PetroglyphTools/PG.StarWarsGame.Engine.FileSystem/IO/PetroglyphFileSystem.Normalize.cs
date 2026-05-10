using System;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    /// <summary>
    /// Rewrites <paramref name="stringBuilder"/> in place so all directory separators are unified
    /// to the host's native form.
    /// </summary>
    /// <remarks>
    /// Both <c>"/"</c> and <c>"\"</c> are recognized as directory separators on every host —
    /// matching the Windows-like path semantics this file system simulates on Linux. The output
    /// uses the System's default directory separator.
    /// </remarks>
    /// <param name="stringBuilder">The buffer whose contents are rewritten in place.</param>
    internal void NormalizePath(ref ValueStringBuilder stringBuilder)
    {
        NormalizePath(stringBuilder.RawChars.Slice(0, stringBuilder.Length));
    }

    private static void NormalizePath(Span<char> path)
    {
        PathNormalizer.Normalize(path, path, PGFileSystemDirectorySeparatorNormalizeOptions);
    }
}