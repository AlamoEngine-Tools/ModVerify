using System;
using PG.StarWarsGame.Engine.Utilities;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    /// <summary>
    /// Resolves <paramref name="filePath"/> against <paramref name="gameDirectory"/>, normalizes
    /// the path, and dispatches the lookup to the active <c>FileExists</c> strategy.
    /// </summary>
    /// <remarks>
    /// Fully-qualified inputs are taken as-is; relative inputs are joined to
    /// <paramref name="gameDirectory"/> first. The buffer is then unified to the host's native
    /// directory separator and dot segments (<c>.</c> and <c>..</c>) are resolved, after which
    /// the active strategy answers the lookup. On <see langword="true" /> the buffer contains
    /// the resolved on-disk path; on <see langword="false" /> the buffer content is unspecified.
    /// </remarks>
    /// <param name="filePath">The path to resolve.</param>
    /// <param name="stringBuilder">
    /// A scratch buffer used to build and return the resolved path. The caller owns the buffer's
    /// lifetime and is responsible for disposing it.
    /// </param>
    /// <param name="gameDirectory">
    /// The game directory used as the base when <paramref name="filePath"/> is relative.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the file exists; otherwise, <see langword="false" />.
    /// </returns>
    internal bool FileExists(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder, ReadOnlySpan<char> gameDirectory)
    {
        stringBuilder.Length = 0;

        if (IsPathFullyQualified_Exists(filePath))
            stringBuilder.Append(filePath);
        else
            JoinPath(gameDirectory, filePath, ref stringBuilder);

        // Canonicalize once for every strategy: unify separators to the host's native form, then
        // strip "." / ".." and trailing/duplicated separators. After this the buffer is ready to
        // hand directly to host FS APIs.
        NormalizePath(ref stringBuilder);
        NormalizeDotSegmentsInPlace(ref stringBuilder);

        return _strategy.FileExists(gameDirectory, ref stringBuilder);
    }
    
    internal void NormalizeDotSegmentsInPlace(ref ValueStringBuilder sb)
    {
        var len = sb.Length;
        if (len == 0)
            return;

        var dirSeparator = _underlyingFileSystem.Path.DirectorySeparatorChar;

        var rootLen = GetPathRoot(sb.AsSpan()).Length;
        var writeEnd = rootLen;
        var readPos = rootLen;

        while (readPos < len && sb[readPos] == dirSeparator)
            readPos++;

        while (readPos < len)
        {
            var segStart = readPos;
            while (readPos < len && sb[readPos] != dirSeparator)
                readPos++;
            var segLen = readPos - segStart;

            while (readPos < len && sb[readPos] == dirSeparator)
                readPos++;

            if (segLen == 1 && sb[segStart] == '.')
                continue;

            if (segLen == 2 && sb[segStart] == '.' && sb[segStart + 1] == '.')
            {
                if (writeEnd > rootLen)
                {
                    while (writeEnd > rootLen && sb[writeEnd - 1] != dirSeparator)
                        writeEnd--;
                    if (writeEnd > rootLen)
                        writeEnd--;
                }
                continue;
            }

            if (writeEnd > rootLen)
                sb[writeEnd++] = dirSeparator;

            for (var i = 0; i < segLen; i++)
                sb[writeEnd++] = sb[segStart + i];
        }

        sb.Length = writeEnd;
    }

    internal bool IsPathFullyQualified_Exists(ReadOnlySpan<char> path)
    {
        // This is really tricky, because under Windows "/" or "\" do NOT
        // indicate a fully qualified path, under Linux however "/" does.
        // The PGFileSystem is implemented to treat backslashes as directory separators.
        // However, this must not happen here, since we are operating on the actual file system.
        // E.g, \\Data\\Art\\... MUST not be treated as a fully qualified path.
        // This means, ultimately, we can just delegate to the underlying file system.
        return _underlyingFileSystem.Path.IsPathFullyQualified(path);
    }
}
