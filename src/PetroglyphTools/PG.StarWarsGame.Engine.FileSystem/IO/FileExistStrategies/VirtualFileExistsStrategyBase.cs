using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal abstract class VirtualFileExistsStrategyBase(IFileSystem fileSystem, FileExistsStrategy underlying)
    : FileExistsStrategy(fileSystem)
{
    protected readonly ConcurrentDictionary<string, VirtualDirectory?> Store =
        new(StringComparer.OrdinalIgnoreCase);

    protected readonly FileExistsStrategy Underlying = underlying;

    internal override void Cleanup()
    {
        Store.Clear();
        Underlying.Cleanup();
    }

    public override bool FileExists(ReadOnlySpan<char> baseDirectory, ref ValueStringBuilder stringBuilder)
    {
        var filePath = stringBuilder.AsSpan();

        if (!IsUnderGameDirectory(filePath, baseDirectory))
            return Underlying.FileExists(baseDirectory, ref stringBuilder);

        var fileName = FileSystem.Path.GetFileName(filePath);
        if (fileName.IsEmpty)
            return false;

        var dirSpan = FileSystem.Path.GetDirectoryName(filePath);
        if (dirSpan.IsEmpty)
            return Underlying.FileExists(baseDirectory, ref stringBuilder);
        
        var dirKey = dirSpan.ToString();
        if (!Store.TryGetValue(dirKey, out var virtualDir))
        {
            virtualDir = TrySnapshot(dirKey);
            Store.TryAdd(dirKey, virtualDir);
        }

        if (virtualDir is null)
            return false;

        if (virtualDir.Files.TryGetValue(fileName.ToString(), out var onDiskName))
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(virtualDir.OnDiskPath);
            if (stringBuilder.Length > 0 && !LowLevelPath.IsDirectorySeparator(stringBuilder[stringBuilder.Length - 1]))
                stringBuilder.Append(FileSystem.Path.DirectorySeparatorChar);
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
            if (pos < path.Length && path[pos] == FileSystem.Path.DirectorySeparatorChar)
                pos++;

            while (pos < path.Length)
            {
                var rest = path.Slice(pos);
                var nextSlash = rest.IndexOf(FileSystem.Path.DirectorySeparatorChar);
                var componentEnd = nextSlash >= 0 ? pos + nextSlash : path.Length;
                var component = path.Slice(pos, componentEnd - pos);

                if (component.IsEmpty)
                {
                    pos = componentEnd + 1;
                    continue;
                }

                var savedLen = sb.Length;
                if (savedLen == 0 || !LowLevelPath.IsDirectorySeparator(sb[savedLen - 1]))
                    sb.Append(FileSystem.Path.DirectorySeparatorChar);
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

    private bool IsUnderGameDirectory(ReadOnlySpan<char> path, ReadOnlySpan<char> gameDirectory)
    {
        return !gameDirectory.IsEmpty && FileSystem.Path.IsChildOf(gameDirectory, path);
    }
}
