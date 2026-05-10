using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class VirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : FileExistsStrategy(fileSystem)
{
    private readonly ConcurrentDictionary<string, VirtualDirectory?> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public override void Dispose()
    {
        _store.Clear();
        underlying.Dispose();
    }

    public override bool FileExists(ReadOnlySpan<char> baseDirectory, ref ValueStringBuilder stringBuilder)
    {
        var filePath = stringBuilder.AsSpan();

        if (!IsUnderBaseDirectory(filePath, baseDirectory))
            return underlying.FileExists(baseDirectory, ref stringBuilder);

        var lastSep = filePath.LastIndexOf(Path.DirectorySeparatorChar);
        if (lastSep <= 0)
            return underlying.FileExists(baseDirectory, ref stringBuilder);

        var dirSpan = filePath.Slice(0, lastSep);
        var fileName = filePath.Slice(lastSep + 1);
        if (fileName.IsEmpty)
            return underlying.FileExists(baseDirectory, ref stringBuilder);

        var dirKey = dirSpan.ToString();
        if (!_store.TryGetValue(dirKey, out var virtualDir))
        {
            virtualDir = TrySnapshot(dirKey);
            _store.TryAdd(dirKey, virtualDir);
        }

        if (virtualDir is null)
            return false;

        if (virtualDir.Files.TryGetValue(fileName.ToString(), out var onDiskName))
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(virtualDir.OnDiskPath);
            if (stringBuilder.Length > 0 && !LowLevelPath.IsDirectorySeparator(stringBuilder[stringBuilder.Length - 1]))
                stringBuilder.Append(Path.DirectorySeparatorChar);
            stringBuilder.Append(onDiskName);
            return true;
        }

        return false;
    }

    private VirtualDirectory? TrySnapshot(string inputDirPath)
    {
        var onDiskPath = TryResolveDirectory(inputDirPath);
        if (onDiskPath is null)
            return null;

        var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in FileSystem.Directory.EnumerateFiles(onDiskPath))
        {
            var name = FileSystem.Path.GetFileName(entry);
            files[name] = name;
        }
        return new VirtualDirectory(onDiskPath, files);
    }

    private string? TryResolveDirectory(string dirPath)
    {
        if (string.IsNullOrEmpty(dirPath))
            return null;

        if (FileSystem.Directory.Exists(dirPath))
            return dirPath;

        var path = dirPath.AsSpan();
        var rootLen = FileSystem.Path.GetPathRoot(path).Length;
        if (rootLen == 0)
            return null;

        var currentDir = dirPath.Substring(0, rootLen);
        if (!FileSystem.Directory.Exists(currentDir))
            return null;

        var sb = new ValueStringBuilder(stackalloc char[260]);
        try
        {
            sb.Append(currentDir);

            var pos = rootLen;
            if (pos < path.Length && path[pos] == Path.DirectorySeparatorChar)
                pos++;

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

                var literalPath = sb.AsSpan().ToString();
                if (FileSystem.Directory.Exists(literalPath))
                {
                    currentDir = literalPath;
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

    private bool IsUnderBaseDirectory(ReadOnlySpan<char> path, ReadOnlySpan<char> gameDirectory)
    {
        return !gameDirectory.IsEmpty && FileSystem.Path.IsChildOf(gameDirectory, path);
    }
}
