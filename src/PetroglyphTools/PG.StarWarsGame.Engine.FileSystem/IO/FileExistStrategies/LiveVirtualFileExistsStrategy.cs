using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class LiveVirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : VirtualFileExistsStrategyBase(fileSystem, underlying)
{
    private const int WatcherBufferSize = 64 * 1024;

    private readonly object _watchersLock = new();
    private readonly Dictionary<string, IFileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public override bool FileExists(ReadOnlySpan<char> baseDirectory, ref ValueStringBuilder stringBuilder)
    {
        if (!baseDirectory.IsEmpty && FileSystem.Path.IsChildOf(baseDirectory, stringBuilder.AsSpan()))
            EnsureWatcher(baseDirectory);
        return base.FileExists(baseDirectory, ref stringBuilder);
    }

    private void EnsureWatcher(ReadOnlySpan<char> baseDirectory)
    {
        if (_disposed)
            return;

        var rootStr = baseDirectory.ToString();
        if (!FileSystem.Directory.Exists(rootStr))
            return;

        lock (_watchersLock)
        {
            if (_disposed || _watchers.ContainsKey(rootStr))
                return;

            var watcher = FileSystem.FileSystemWatcher.New(rootStr);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.InternalBufferSize = WatcherBufferSize;

            watcher.Created += OnFileEvent;
            watcher.Deleted += OnFileEvent;
            watcher.Changed += OnFileEvent;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnWatcherError;

            watcher.EnableRaisingEvents = true;
            _watchers[rootStr] = watcher;
        }
    }

    public override void Dispose()
    {
        IFileSystemWatcher[] watchers;
        lock (_watchersLock)
        {
            if (_disposed)
                return;
            _disposed = true;
            watchers = new IFileSystemWatcher[_watchers.Count];
            _watchers.Values.CopyTo(watchers, 0);
            _watchers.Clear();
        }

        foreach (var watcher in watchers)
            TearDownWatcher(watcher);

        base.Dispose();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        InvalidatePathAndSubtree(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        InvalidatePathAndSubtree(e.OldFullPath);
        InvalidatePathAndSubtree(e.FullPath);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        IFileSystemWatcher? broken = null;
        string? brokenRoot = null;
        lock (_watchersLock)
        {
            if (_disposed)
                return;
            foreach (var kv in _watchers)
            {
                if (ReferenceEquals(kv.Value, sender))
                {
                    broken = kv.Value;
                    brokenRoot = kv.Key;
                    break;
                }
            }
            if (broken is null)
                return;
            _watchers.Remove(brokenRoot!);
        }

        ClearCacheUnder(brokenRoot!);
        TearDownWatcher(broken);
    }

    private void TearDownWatcher(IFileSystemWatcher watcher)
    {
        watcher.EnableRaisingEvents = false;
        watcher.Created -= OnFileEvent;
        watcher.Deleted -= OnFileEvent;
        watcher.Changed -= OnFileEvent;
        watcher.Renamed -= OnFileRenamed;
        watcher.Error -= OnWatcherError;
        watcher.Dispose();
    }

    private void ClearCacheUnder(string root)
    {
        Store.TryRemove(root, out _);
        var prefix = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? root
            : root + Path.DirectorySeparatorChar;
        foreach (var key in Store.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                Store.TryRemove(key, out _);
        }
    }

    private void InvalidatePathAndSubtree(string fullPath)
    {
        InvalidateParentOf(fullPath);
        InvalidateSubtree(fullPath);
    }

    private void InvalidateParentOf(string fullPath)
    {
        var parent = FileSystem.Path.GetDirectoryName(fullPath);
        if (parent is { Length: > 0 })
            Store.TryRemove(parent, out _);
    }

    private void InvalidateSubtree(string fullPath)
    {
        Store.TryRemove(fullPath, out _);
        var prefix = fullPath + Path.DirectorySeparatorChar;
        foreach (var key in Store.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                Store.TryRemove(key, out _);
        }
    }
}
