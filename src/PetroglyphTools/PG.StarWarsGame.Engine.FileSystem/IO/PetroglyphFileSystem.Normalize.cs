using System;
using System.Diagnostics;
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
        PathNormalizer.Normalize(path, path, PGFileSystemDirectorySeparatorNormalizeOptions);
    }

    /// <summary>
    /// Resolves "." and ".." segments inside <paramref name="sb"/> in a single forward pass
    /// without allocating. Also collapses runs of consecutive separators ("//") and strips a
    /// trailing separator. ".." segments pop the previous resolved segment, clamped at root.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The path MUST be rooted (i.e. start with '/'). For relative paths, the meaning of a
    /// leading ".." depends on a CWD that this method does not know, so resolution there is
    /// undefined and rejected by the contract.
    /// </para>
    /// <para>
    /// Two-pointer rewrite: <c>readPos</c> scans forward, <c>writeEnd</c> trails. Because
    /// <c>writeEnd ≤ readPos</c> at every step, writes never clobber chars still to be read.
    /// </para>
    /// </remarks>
    /// <param name="sb">Buffer to rewrite in place. Must contain a rooted path.</param>
    internal static void NormalizeDotSegmentsInPlace(ref ValueStringBuilder sb)
    {
        var len = sb.Length;
        if (len == 0)
            return;

        Debug.Assert(sb[0] == '/', "NormalizeDotSegmentsInPlace requires a rooted path (starts with '/').");

        const int rootLen = 1;
        var writeEnd = rootLen;
        var readPos = rootLen;

        // Skip any leading consecutive slashes after the root.
        while (readPos < len && sb[readPos] == '/')
            readPos++;

        while (readPos < len)
        {
            var segStart = readPos;
            while (readPos < len && sb[readPos] != '/')
                readPos++;
            var segLen = readPos - segStart;

            // Consume separator slashes after this segment (collapses "//" to "/").
            while (readPos < len && sb[readPos] == '/')
                readPos++;

            // "." — drop.
            if (segLen == 1 && sb[segStart] == '.')
                continue;

            // ".." — pop the previously written segment plus the slash before it.
            // Clamps at root: a ".." that would cross root is silently absorbed.
            if (segLen == 2 && sb[segStart] == '.' && sb[segStart + 1] == '.')
            {
                if (writeEnd > rootLen)
                {
                    while (writeEnd > rootLen && sb[writeEnd - 1] != '/')
                        writeEnd--;
                    if (writeEnd > rootLen)
                        writeEnd--;
                }
                continue;
            }

            // Normal segment — keep. Insert a separator before any segment past root.
            if (writeEnd > rootLen)
                sb[writeEnd++] = '/';

            for (var i = 0; i < segLen; i++)
                sb[writeEnd++] = sb[segStart + i];
        }

        sb.Length = writeEnd;
    }
}