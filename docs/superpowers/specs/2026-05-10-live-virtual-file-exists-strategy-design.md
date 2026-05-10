# Live Virtual File-Exists Strategy

**Status:** Design approved, awaiting implementation plan
**Date:** 2026-05-10
**Component:** `PG.StarWarsGame.Engine.FileSystem`

## Problem

`VirtualFileExistsStrategy` builds a per-directory snapshot of file listings on first lookup and caches it for the lifetime of the strategy. The snapshot never refreshes. For consumers that hold the strategy alive across externally-driven file system changes (mod editors, an embedded engine host), a deleted or newly-added file under the game directory remains undetected until the strategy is recreated.

ModVerify itself does not need this — it scans once and disposes — but `PG.StarWarsGame.Engine.FileSystem` is a general-purpose library and other consumers do.

## Goals

- Add an opt-in variant of the virtual strategy that keeps its snapshots fresh under file additions, deletions, and renames within the game directory.
- Avoid creating one watcher per cached directory; one process-wide watcher per strategy instance.
- Guarantee that the watcher's invalidation handler and concurrent `FileExists` callers do not race on shared mutable state.
- Stay opt-in. Default behavior (`UseVirtualStrategy`) is unchanged.

## Non-goals

- Tracking file *content* changes. `FileExists` only reports existence; mtime/size do not affect correctness.
- Notifying consumers of file system changes (no public events, no callbacks).
- A "live" variant for `WindowsFileExistsStrategy` or `WineFileExistsStrategy`. Those strategies do not cache, so live mode is meaningless for them.
- Debouncing / coalescing event bursts at the watcher layer. Lazy rebuild already coalesces: N events on the same directory collapse to a single re-snapshot at next access.
- Filtering to a directory subtree narrower than the game directory.
- A periodic rescan / TTL backstop.

## Design

### API

A new public method on `PetroglyphFileSystem`:

```csharp
public void UseLiveVirtualStrategy(bool? windowsFallback = null);
```

Signature mirrors `UseVirtualStrategy`'s nullable form: `null` selects the Windows strategy on Windows hosts and the Wine strategy otherwise; an explicit `true` forces the Windows strategy and throws `PlatformNotSupportedException` on non-Windows hosts; an explicit `false` forces the Wine strategy on every host.

Selecting it swaps the active strategy via the existing `SwapStrategy` path, which disposes the previous strategy.

`UseVirtualStrategy` retains its (nullable) signature and behavior — purely additive surface change.

### Components

A new internal class `LiveVirtualFileExistsStrategy : FileExistsStrategy`, a sibling of `VirtualFileExistsStrategy` in `PG.StarWarsGame.Engine.IO.FileExistStrategies`. It holds:

- The same `ConcurrentDictionary<string, VirtualDirectory?> _store` keyed by directory path (case-insensitive), with the same `VirtualDirectory` (immutable, files-only, on-disk casing) value type as the non-live strategy.
- The same `underlying` fallback strategy passed in by `PetroglyphFileSystem.UseLiveVirtualStrategy`.
- A `FileSystemWatcher? _watcher`, started lazily.
- A `string? _watchedRoot` recording the game directory the watcher was bound to.

The directory snapshot logic (`TrySnapshot`, `TryResolveDirectory`, `IsUnderGameDirectory`) is identical to `VirtualFileExistsStrategy`. To avoid duplication, those helpers move to a `protected` location on a shared abstract base or a static helper class. The exact split is an implementation-plan concern, not a design decision.

### Watcher lifecycle

The watcher is created **lazily** on the first `FileExists` call that lands inside the game directory — that is the first point at which the strategy learns the game directory path (it is passed per-call, not at construction).

```csharp
public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder sb)
{
    if (IsUnderGameDirectory(sb.AsSpan(), gameDirectory))
        EnsureWatcher(gameDirectory);
    // ... rest identical to VirtualFileExistsStrategy
}
```

`EnsureWatcher` is idempotent: it only constructs a watcher when `_watcher` is null **and** the resolved game directory exists on disk. The watcher binds to the first observed game directory and stays bound to it for the strategy's lifetime — `gameDirectory` changing mid-flight is not an expected operating mode for this library, and the strategy makes no attempt to detect or react to it. Consumers who change the game directory should swap the strategy (which disposes this one and tears down the watcher).

Watcher configuration:

| Property                | Value |
|-------------------------|-------|
| `Path`                  | resolved game directory |
| `IncludeSubdirectories` | `true` |
| `NotifyFilters`         | `FileName \| DirectoryName` |
| `InternalBufferSize`    | `65536` (64 KB; default is 8 KB) |
| `EnableRaisingEvents`   | `true` after subscribing |

Subscribed events: `Created`, `Deleted`, `Renamed`, `Changed`, `Error`.

`Dispose()` sets `EnableRaisingEvents = false`, disposes the watcher, clears `_store`, and disposes the underlying.

### Concurrency model — lazy invalidation

The watcher handler does **one** thing: remove the affected directory's entry from `_store`. It never rebuilds and never blocks on a `FileExists` call.

```csharp
private void OnFsEvent(string fullPath)
{
    var dirKey = FileSystem.Path.GetDirectoryName(fullPath);
    if (dirKey is not null)
        _store.TryRemove(dirKey, out _);
}
```

Two facts make this race-free without locking:

1. `VirtualDirectory` is already immutable.
2. `ConcurrentDictionary` operations (`TryRemove`, `TryAdd`, `TryGetValue`) are atomic.

Therefore a concurrent `FileExists` mid-lookup either:

- already captured the now-stale `VirtualDirectory` reference (returns a slightly-stale-but-consistent answer — no torn read because the snapshot is immutable), **or**
- finds the entry gone on its next `TryGetValue` and rebuilds via `TrySnapshot`.

No two threads ever mutate the same `VirtualDirectory`. The watcher thread never blocks on application work.

### Event mapping

| Event                               | Action |
|-------------------------------------|--------|
| `Created` / `Deleted` / `Changed` (file) | `TryRemove(parentDir(fullPath))` |
| `Renamed` (file)                    | `TryRemove(parentDir(oldPath))` and `TryRemove(parentDir(newPath))` |
| `Deleted` / `Renamed` (directory)   | `TryRemove(dirPath)` and `TryRemove(k)` for every `k` in `_store.Keys` where `k` is `dirPath` or starts with `dirPath + sep` (case-insensitive) |
| `Created` (directory)               | No action. Uncached dirs cost nothing; first lookup will snapshot. |
| `Error`                             | `_store.Clear()` (see Reliability) |

`Changed` is included even though content changes do not matter, because some platforms surface delete/create as `Changed` on the parent. The handler is the same `TryRemove` call regardless — cheap.

The directory-deletion case is the only one that walks `_store.Keys`. The cache is small (at most the directories actually consulted by the engine, typically O(10²)), so the linear scan is fine.

### Reliability backstop

`FileSystemWatcher` drops events when its internal buffer overflows. On Linux, inotify has per-user/per-instance limits that get tripped silently under heavy churn.

Two mitigations, both standard and inexpensive:

1. **Raise `InternalBufferSize` to 64 KB.** Default is 8 KB; the larger buffer measurably reduces overflow under bursty changes.
2. **Subscribe to `FileSystemWatcher.Error`.** On any error event, call `_store.Clear()`. Every subsequent `FileExists` rebuilds its directory snapshot from disk. This is the canonical FSW recovery pattern and does not introduce any periodic work.

No TTL, no periodic rescan, no per-entry timestamp checks. `Error → Clear` is the only backstop.

### Disposal

```csharp
public override void Dispose()
{
    var w = _watcher;
    _watcher = null;
    if (w is not null)
    {
        w.EnableRaisingEvents = false;
        w.Created -= OnCreatedOrDeletedOrChanged;
        // ... unsubscribe all
        w.Dispose();
    }
    _store.Clear();
    underlying.Dispose();
}
```

`PetroglyphFileSystem.SwapStrategy` already disposes the previous strategy, so a consumer calling `UseLiveVirtualStrategy` then later `UseWindowsStrategy` (or recreating the file system) cleanly tears down the watcher.

## Testing

Unit tests live alongside the existing `VirtualFileExistsStrategyTests` in `PG.StarWarsGame.Engine.FileSystem.Test`.

- Use the existing `MockFileSystem` for the snapshot logic where possible.
- For watcher behavior, the watcher is wired against a real OS path via the test's temp directory infrastructure (`MockFileSystem` does not raise FSW events). One integration-style test class, kept small.

Required scenarios:

- A file created on disk under a previously-cached directory is reported as existing on the next `FileExists` call (after a brief polling wait for the FSW event).
- A file deleted on disk under a previously-cached directory is reported as missing on the next `FileExists` call.
- A renamed file is reported under its new name and not its old name on the next call.
- A renamed directory invalidates its own snapshot and any cached descendants.
- A deleted directory invalidates its own snapshot and any cached descendants.
- After `Dispose`, no further file-system events touch the store (verify by sleeping past a created file and asserting no exception / no store mutation — use a wrapping store you can spy on).
- Lookups outside the game directory continue to delegate to the underlying strategy unchanged.
- `UseLiveVirtualStrategy(windowsFallback: true)` throws `PlatformNotSupportedException` on non-Windows hosts (parity with `UseVirtualStrategy`).
- `UseLiveVirtualStrategy()` (default `null`) selects the Windows fallback on Windows and the Wine fallback elsewhere.
- `UseLiveVirtualStrategy(windowsFallback: false)` selects the Wine fallback on every host, including Windows.

## Risks

- **FSW event timing in tests.** FSW events are asynchronous; tests must poll with a timeout rather than asserting immediately. Standard pattern.
- **Linux inotify watch limits.** A consumer holding many `LiveVirtualFileExistsStrategy` instances on Linux could exhaust inotify watches. Mitigation: this is an opt-in API; consumers spinning up many engine hosts should size their inotify limits accordingly. Documented in the XML doc.
- **Path normalization at the event boundary.** `FileSystemWatcher` reports paths in OS form. The store keys are also OS-form (built from `Path.GetDirectoryName` and `Path.Combine` on the underlying file system). They must compare under the same normalization rules the rest of the strategy uses (`OrdinalIgnoreCase`). The existing dictionary already uses `StringComparer.OrdinalIgnoreCase`; the watcher handler must use `Path.GetDirectoryName` from the same `IFileSystem` to keep separator handling consistent.
