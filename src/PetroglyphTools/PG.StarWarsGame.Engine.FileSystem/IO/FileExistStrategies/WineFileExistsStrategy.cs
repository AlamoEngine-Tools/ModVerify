using System;
using System.IO;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine.Utilities;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class WineFileExistsStrategy(IFileSystem fileSystem) : FileExistsStrategy(fileSystem)
{
    public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder stringBuilder)
    {
        var pathString = stringBuilder.AsSpan().ToString();
        if (pathString.Length == 0)
            return false;

        if (FileSystem.File.Exists(pathString))
            return true;

        var path = pathString.AsSpan();

        var lastSep = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (lastSep < 0)
            return false;

        var fileName = path.Slice(lastSep + 1);
        if (fileName.IsEmpty)
            return false;

        var rootLen = FileSystem.Path.GetPathRoot(path).Length;
        var parentLen = Math.Max(lastSep, rootLen);
        if (parentLen == 0)
            return false;
        var parentDirInput = pathString.Substring(0, parentLen);

        var resolvedParent = ResolveDirectory(parentDirInput, gameDirectory, rootLen);
        if (resolvedParent is null)
            return false;

        return ResolveLeafIn(resolvedParent, fileName, ref stringBuilder);
    }

    private string? ResolveDirectory(string dirInput, ReadOnlySpan<char> gameDirectory, int rootLen)
    {
        var path = dirInput.AsSpan();
        var knownGoodPrefixLength = LowLevelPath.GetCommonDirectoryPrefixLength(path, gameDirectory);

        int prefixEnd;
        if (knownGoodPrefixLength > rootLen)
        {
            prefixEnd = Math.Min(knownGoodPrefixLength, path.Length);
            while (prefixEnd > rootLen && path[prefixEnd - 1] == Path.DirectorySeparatorChar)
                prefixEnd--;
        }
        else
        {
            prefixEnd = rootLen;
        }

        if (prefixEnd == 0)
            return null;

        var currentDir = dirInput.Substring(0, prefixEnd);
        if (!FileSystem.Directory.Exists(currentDir))
            return null;

        var pos = prefixEnd;
        if (pos < path.Length && path[pos] == Path.DirectorySeparatorChar)
            pos++;

        var sb = new ValueStringBuilder(stackalloc char[260]);
        try
        {
            sb.Append(currentDir);

            while (pos < path.Length)
            {
                var rest = path.Slice(pos);
                var nextSlash = rest.IndexOf(Path.DirectorySeparatorChar);
                var componentEnd = nextSlash >= 0 ? pos + nextSlash : path.Length;
                var component = path.Slice(pos, componentEnd - pos);

                if (component.IsEmpty)
                {
                    pos = componentEnd + 1;
                    continue;
                }

                var savedLen = sb.Length;
                if (savedLen == 0 || !LowLevelPath.IsDirectorySeparator(sb[savedLen - 1]))
                    sb.Append(Path.DirectorySeparatorChar);
                sb.Append(component);

                var literalAttempt = sb.AsSpan().ToString();
                if (FileSystem.Directory.Exists(literalAttempt))
                {
                    currentDir = literalAttempt;
                    pos = componentEnd + 1;
                    continue;
                }

                sb.Length = savedLen;

                var found = false;
                foreach (var entry in FileSystem.Directory.EnumerateDirectories(currentDir))
                {
                    if (FileSystem.Path.GetFileName(entry.AsSpan()).Equals(component, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Length = 0;
                        sb.Append(entry);
                        currentDir = entry;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return null;

                pos = componentEnd + 1;
            }

            return currentDir;
        }
        finally
        {
            sb.Dispose();
        }
    }

    private bool ResolveLeafIn(string parentOnDisk, ReadOnlySpan<char> fileName, ref ValueStringBuilder outBuffer)
    {
        var sb = new ValueStringBuilder(stackalloc char[260]);
        try
        {
            sb.Append(parentOnDisk);
            if (sb.Length == 0 || !LowLevelPath.IsDirectorySeparator(sb[sb.Length - 1]))
                sb.Append(Path.DirectorySeparatorChar);
            sb.Append(fileName);

            var literalAttempt = sb.AsSpan().ToString();
            if (FileSystem.File.Exists(literalAttempt))
            {
                outBuffer.Length = 0;
                outBuffer.Append(literalAttempt);
                return true;
            }
        }
        finally
        {
            sb.Dispose();
        }

        foreach (var entry in FileSystem.Directory.EnumerateFiles(parentOnDisk))
        {
            if (FileSystem.Path.GetFileName(entry.AsSpan()).Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                outBuffer.Length = 0;
                outBuffer.Append(entry);
                return true;
            }
        }

        return false;
    }
}
