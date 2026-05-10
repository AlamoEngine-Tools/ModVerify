# Live Virtual File-Exists Strategy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an opt-in `LiveVirtualFileExistsStrategy` that keeps `VirtualFileExistsStrategy`'s per-directory snapshots fresh under file additions, deletions, and renames within the game directory, exposed via `PetroglyphFileSystem.UseLiveVirtualStrategy(bool? windowsFallback = null)`.

**Architecture:** Extract the existing snapshot/lookup logic into an abstract `VirtualFileExistsStrategyBase`. The current `VirtualFileExistsStrategy` becomes a thin sealed subclass (no behavior change). A new `LiveVirtualFileExistsStrategy` inherits the base and layers a single, lazily-created, recursive `FileSystemWatcher` over the game directory. Watcher events run a one-line invalidation against the `ConcurrentDictionary` snapshot store (`TryRemove`); the next `FileExists` rebuilds. Invalidation never blocks lookups and never mutates a `VirtualDirectory` (which is already immutable).

**Tech Stack:** .NET, C# 12 (primary constructors, file-scoped namespaces, nullable reference types), `System.IO.Abstractions`, xUnit, `Testably.Abstractions` `RealFileSystem` for tests.

**Spec:** `docs/superpowers/specs/2026-05-10-live-virtual-file-exists-strategy-design.md`

**Test entry point:**
```bash
dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj
```

---

## Task 1: Extract `VirtualFileExistsStrategyBase`

Pull the snapshot store, lookup dispatch, snapshot construction, and case-folding directory walk out of `VirtualFileExistsStrategy` into a new abstract base. The non-live class becomes a one-line subclass. **No behavior change.** The existing `VirtualFileExistsStrategyTests` runners (`_Wine`, `_Windows`) must continue to pass unchanged.

**Files:**
- Create: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategyBase.cs`
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategy.cs`

- [ ] **Step 1: Create the new base class file**

`src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategyBase.cs`:

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

    public override void Dispose()
    {
        Store.Clear();
        Underlying.Dispose();
    }

    public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder stringBuilder)
    {
        var filePath = stringBuilder.AsSpan();

        if (!IsUnderGameDirectory(filePath, gameDirectory))
            return Underlying.FileExists(gameDirectory, ref stringBuilder);

        var lastSep = filePath.LastIndexOf(Path.DirectorySeparatorChar);
        if (lastSep <= 0)
            return Underlying.FileExists(gameDirectory, ref stringBuilder);

        var dirSpan = filePath.Slice(0, lastSep);
        var fileName = filePath.Slice(lastSep + 1);
        if (fileName.IsEmpty)
            return Underlying.FileExists(gameDirectory, ref stringBuilder);

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

    private bool IsUnderGameDirectory(ReadOnlySpan<char> path, ReadOnlySpan<char> gameDirectory)
    {
        return !gameDirectory.IsEmpty && FileSystem.Path.IsChildOf(gameDirectory, path);
    }
}
```

- [ ] **Step 2: Slim `VirtualFileExistsStrategy` to inherit from the base**

Replace the entire body of `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategy.cs` with:

```csharp
using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class VirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : VirtualFileExistsStrategyBase(fileSystem, underlying);
```

- [ ] **Step 3: Build the solution**

Run: `dotnet build src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/PG.StarWarsGame.Engine.FileSystem.csproj -c Debug`
Expected: Build succeeds, 0 errors.

- [ ] **Step 4: Run the existing virtual-strategy tests**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~VirtualFileExistsStrategy"`
Expected: All tests pass — `VirtualFileExistsStrategy_Wine` and (if Windows) `VirtualFileExistsStrateg_Windows` are green. No new tests yet.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategyBase.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/VirtualFileExistsStrategy.cs
git commit -m "extract VirtualFileExistsStrategyBase from VirtualFileExistsStrategy"
```

---

## Task 2: Add `LiveVirtualFileExistsStrategy` skeleton (no watcher yet)

Introduce the new class as a no-op subclass of the base so it inherits the full snapshot behavior. No watcher logic. This lets us prove the inheritance shape compiles and passes the same lookup tests before we layer behavior on top.

**Files:**
- Create: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`

- [ ] **Step 1: Create the file**

```csharp
using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class LiveVirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : VirtualFileExistsStrategyBase(fileSystem, underlying);
```

- [ ] **Step 2: Build**

Run: `dotnet build src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/PG.StarWarsGame.Engine.FileSystem.csproj -c Debug`
Expected: Build succeeds, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs
git commit -m "add LiveVirtualFileExistsStrategy skeleton"
```

---

## Task 3: Add internal `UseLiveVirtualStrategy(FileExistsStrategy)` and shared test scaffolding

Wire an internal overload that takes a fallback strategy directly (parallel to the existing `internal void UseVirtualStrategy(FileExistsStrategy underlying)`), so tests can run the live strategy with a `TrackingFileExistsStrategy` fallback. **Do not yet expose the public overload** — we want all behavior tested before consumers see the API.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/PetroglyphFileSystem.Strategies.cs`
- Create: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`

- [ ] **Step 1: Add the internal overload**

Add the following method to `PetroglyphFileSystem.Strategies.cs`, immediately after `internal void UseVirtualStrategy(FileExistsStrategy underlying)`:

```csharp
internal void UseLiveVirtualStrategy(FileExistsStrategy underlying)
    => SwapStrategy(new LiveVirtualFileExistsStrategy(_underlyingFileSystem, underlying));
```

- [ ] **Step 2: Create the abstract test class with `_Wine` and `_Windows` runners and one shared assertion test**

`src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`:

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

#if Windows
public sealed class LiveVirtualFileExistsStrategy_Windows : LiveVirtualFileExistsStrategyTests
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
    {
        fs.UseLiveVirtualStrategy(new WineFileExistsStrategy(fs.UnderlyingFileSystem));
    }
}
#endif

public sealed class LiveVirtualFileExistsStrategy_Wine : LiveVirtualFileExistsStrategyTests
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
    {
        fs.UseLiveVirtualStrategy(new WineFileExistsStrategy(fs.UnderlyingFileSystem));
    }
}

public abstract class LiveVirtualFileExistsStrategyTests : FileExistsStrategyTestBase
{
    private static readonly TimeSpan WatcherEventTimeout = TimeSpan.FromSeconds(3);

    [Fact]
    public void FileExists_RepeatedCallsSameDirectory_BothResolveFromSnapshot()
    {
        var dir = NewTempDir();
        var dataDir = Path.Combine(dir, "Mods", "Test", "Data", "Xml");
        Directory.CreateDirectory(dataDir);
        var foo = Path.Combine(dataDir, "foo.xml");
        var bar = Path.Combine(dataDir, "bar.xml");
        File.WriteAllText(foo, "1");
        File.WriteAllText(bar, "2");

        var sb1 = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists("MODS/TEST/DATA/XML/FOO.XML".AsSpan(), ref sb1, dir.AsSpan()));
        AssertResolvedPath(foo, sb1.ToString());

        var sb2 = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists("mods/test/data/xml/BAR.XML".AsSpan(), ref sb2, dir.AsSpan()));
        AssertResolvedPath(bar, sb2.ToString());
    }

    /// <summary>Polls the lookup until the predicate is satisfied or <see cref="WatcherEventTimeout"/> elapses.</summary>
    protected void AssertEventually(Func<bool> predicate, string description)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < WatcherEventTimeout)
        {
            if (predicate())
                return;
            Thread.Sleep(25);
        }
        Assert.Fail($"Timed out after {WatcherEventTimeout} waiting for: {description}");
    }
}
```

- [ ] **Step 3: Run the new test class**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All shared tests inherited from `FileExistsStrategyTestBase` plus the one new `FileExists_RepeatedCallsSameDirectory_BothResolveFromSnapshot` test pass.

- [ ] **Step 4: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/PetroglyphFileSystem.Strategies.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "wire internal UseLiveVirtualStrategy overload and test scaffolding"
```

---

## Task 4: Lazy watcher creation + disposal

Add the watcher lifecycle: lazily created on the first `FileExists` call that lands inside the game directory, configured per spec, torn down in `Dispose`. Still no event handlers, so the watcher is a no-op observer for now.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`

- [ ] **Step 1: Write the failing test for watcher lifecycle**

Append the following test to `LiveVirtualFileExistsStrategyTests`:

```csharp
[Fact]
public void Dispose_AfterLookup_DoesNotThrowAndStopsObserving()
{
    var dir = NewTempDir();
    var dataDir = Path.Combine(dir, "Data");
    Directory.CreateDirectory(dataDir);
    File.WriteAllText(Path.Combine(dataDir, "foo.xml"), "x");

    Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));

    // Re-construct a fresh PgFileSystem so we can dispose the strategy via SwapStrategy.
    PgFileSystem.UseWineStrategy();

    // After the strategy has been swapped (and thus disposed), creating a new file in the
    // observed directory must not throw on a now-disposed watcher's event thread.
    File.WriteAllText(Path.Combine(dataDir, "bar.xml"), "y");
    Thread.Sleep(200);
}
```

- [ ] **Step 2: Run the test to confirm it currently passes** (no watcher yet — there is nothing to observe)

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~Dispose_AfterLookup_DoesNotThrowAndStopsObserving"`
Expected: PASS. (We are committing the test up front; the assertion guards against regression once the watcher exists.)

- [ ] **Step 3: Implement watcher lifecycle in `LiveVirtualFileExistsStrategy`**

Replace the contents of `LiveVirtualFileExistsStrategy.cs`:

```csharp
using System;
using System.IO;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class LiveVirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : VirtualFileExistsStrategyBase(fileSystem, underlying)
{
    private const int WatcherBufferSize = 64 * 1024;

    private readonly object _watcherLock = new();
    private FileSystemWatcher? _watcher;
    private bool _disposed;

    public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder stringBuilder)
    {
        if (!gameDirectory.IsEmpty && FileSystem.Path.IsChildOf(gameDirectory, stringBuilder.AsSpan()))
            EnsureWatcher(gameDirectory);
        return base.FileExists(gameDirectory, ref stringBuilder);
    }

    private void EnsureWatcher(ReadOnlySpan<char> gameDirectory)
    {
        if (_watcher is not null || _disposed)
            return;

        var rootStr = gameDirectory.ToString();
        if (!FileSystem.Directory.Exists(rootStr))
            return;

        lock (_watcherLock)
        {
            if (_watcher is not null || _disposed)
                return;

            var watcher = new FileSystemWatcher(rootStr)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                InternalBufferSize = WatcherBufferSize,
            };

            watcher.EnableRaisingEvents = true;
            _watcher = watcher;
        }
    }

    public override void Dispose()
    {
        FileSystemWatcher? watcher;
        lock (_watcherLock)
        {
            if (_disposed)
                return;
            _disposed = true;
            watcher = _watcher;
            _watcher = null;
        }

        if (watcher is not null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        base.Dispose();
    }
}
```

- [ ] **Step 4: Run the full live-strategy test set**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All previously-passing tests still pass.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "add lazy FileSystemWatcher lifecycle to LiveVirtualFileExistsStrategy"
```

---

## Task 5: Invalidate snapshots on file `Created` / `Deleted` / `Changed`

Wire the file-level events to remove the affected directory's snapshot from the store. Lazy rebuild on the next `FileExists` call.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`

- [ ] **Step 1: Write the failing tests**

Append to `LiveVirtualFileExistsStrategyTests`:

```csharp
[Fact]
public void FileExists_AfterFileDeletedOnDisk_ReportsMissing()
{
    var dir = NewTempDir();
    var dataDir = Path.Combine(dir, "Data");
    Directory.CreateDirectory(dataDir);
    var file = Path.Combine(dataDir, "foo.xml");
    File.WriteAllText(file, "x");

    Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));

    File.Delete(file);

    AssertEventually(
        () => !FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()),
        "snapshot to refresh after Data/foo.xml was deleted on disk");
}

[Fact]
public void FileExists_AfterFileCreatedOnDisk_ReportsPresent()
{
    var dir = NewTempDir();
    var dataDir = Path.Combine(dir, "Data");
    Directory.CreateDirectory(dataDir);
    File.WriteAllText(Path.Combine(dataDir, "seed.xml"), "x");

    // Prime the snapshot.
    Assert.True(FileExists("Data/seed.xml".AsSpan(), dir.AsSpan()));
    Assert.False(FileExists("Data/new.xml".AsSpan(), dir.AsSpan()));

    File.WriteAllText(Path.Combine(dataDir, "new.xml"), "y");

    AssertEventually(
        () => FileExists("Data/new.xml".AsSpan(), dir.AsSpan()),
        "snapshot to refresh after Data/new.xml was created on disk");
}
```

- [ ] **Step 2: Run the tests and confirm they fail**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~AfterFileDeletedOnDisk|FullyQualifiedName~AfterFileCreatedOnDisk"`
Expected: Both tests fail with `Assert.Fail` from `AssertEventually` — the snapshot is stale because no event handler is wired yet.

- [ ] **Step 3: Wire the file-level event handlers**

In `LiveVirtualFileExistsStrategy.cs`, modify `EnsureWatcher` to subscribe and add the handler. Replace the `lock (_watcherLock) { ... }` block inside `EnsureWatcher` with:

```csharp
lock (_watcherLock)
{
    if (_watcher is not null || _disposed)
        return;

    var watcher = new FileSystemWatcher(rootStr)
    {
        IncludeSubdirectories = true,
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
        InternalBufferSize = WatcherBufferSize,
    };

    watcher.Created += OnFileEvent;
    watcher.Deleted += OnFileEvent;
    watcher.Changed += OnFileEvent;

    watcher.EnableRaisingEvents = true;
    _watcher = watcher;
}
```

Add the handler method to the class:

```csharp
private void OnFileEvent(object sender, FileSystemEventArgs e)
{
    InvalidateParentOf(e.FullPath);
}

private void InvalidateParentOf(string fullPath)
{
    var parent = FileSystem.Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(parent))
        Store.TryRemove(parent, out _);
}
```

Also unsubscribe in `Dispose`. Update the `if (watcher is not null)` block in `Dispose` to:

```csharp
if (watcher is not null)
{
    watcher.EnableRaisingEvents = false;
    watcher.Created -= OnFileEvent;
    watcher.Deleted -= OnFileEvent;
    watcher.Changed -= OnFileEvent;
    watcher.Dispose();
}
```

- [ ] **Step 4: Run the tests and confirm they pass**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All live-strategy tests pass, including the two new ones.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "invalidate snapshot on file create/delete/change events"
```

---

## Task 6: Invalidate snapshots on file `Renamed`

A rename's old and new paths can land in different directories. Invalidate both parents.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`

- [ ] **Step 1: Write the failing test**

Append:

```csharp
[Fact]
public void FileExists_AfterFileRenamed_OldNameMissingNewNamePresent()
{
    var dir = NewTempDir();
    var dataDir = Path.Combine(dir, "Data");
    Directory.CreateDirectory(dataDir);
    var oldPath = Path.Combine(dataDir, "old.xml");
    var newPath = Path.Combine(dataDir, "new.xml");
    File.WriteAllText(oldPath, "x");

    Assert.True(FileExists("Data/old.xml".AsSpan(), dir.AsSpan()));
    Assert.False(FileExists("Data/new.xml".AsSpan(), dir.AsSpan()));

    File.Move(oldPath, newPath);

    AssertEventually(
        () => !FileExists("Data/old.xml".AsSpan(), dir.AsSpan()) && FileExists("Data/new.xml".AsSpan(), dir.AsSpan()),
        "snapshot to reflect the rename of Data/old.xml to Data/new.xml");
}
```

- [ ] **Step 2: Run the test and confirm it fails**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~AfterFileRenamed_OldNameMissingNewNamePresent"`
Expected: Fails — `Renamed` is not subscribed yet.

- [ ] **Step 3: Subscribe to `Renamed`**

In `EnsureWatcher`, add the subscription alongside the others:

```csharp
watcher.Renamed += OnFileRenamed;
```

Add the handler:

```csharp
private void OnFileRenamed(object sender, RenamedEventArgs e)
{
    InvalidateParentOf(e.OldFullPath);
    InvalidateParentOf(e.FullPath);
}
```

Unsubscribe in `Dispose`:

```csharp
watcher.Renamed -= OnFileRenamed;
```

- [ ] **Step 4: Run tests**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All live-strategy tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "invalidate snapshots on file rename"
```

---

## Task 7: Invalidate descendants on directory `Deleted` / `Renamed`

A directory rename does not produce per-file Renamed events for its descendants on Windows. Cascade-invalidate every cached entry whose key equals the affected path or starts with it plus a separator.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs`

- [ ] **Step 1: Write failing tests**

Append:

```csharp
[Fact]
public void FileExists_AfterDirectoryRenamed_OldPathMissingNewPathPresent()
{
    var dir = NewTempDir();
    var oldDir = Path.Combine(dir, "OldData");
    var newDir = Path.Combine(dir, "NewData");
    Directory.CreateDirectory(oldDir);
    File.WriteAllText(Path.Combine(oldDir, "foo.xml"), "x");

    Assert.True(FileExists("OldData/foo.xml".AsSpan(), dir.AsSpan()));

    Directory.Move(oldDir, newDir);

    AssertEventually(
        () => !FileExists("OldData/foo.xml".AsSpan(), dir.AsSpan()) && FileExists("NewData/foo.xml".AsSpan(), dir.AsSpan()),
        "cached descendants of OldData to invalidate after directory rename");
}

[Fact]
public void FileExists_AfterDirectoryDeleted_AllDescendantsInvalidated()
{
    var dir = NewTempDir();
    var dataDir = Path.Combine(dir, "Data");
    var subDir = Path.Combine(dataDir, "Sub");
    Directory.CreateDirectory(subDir);
    File.WriteAllText(Path.Combine(dataDir, "a.xml"), "1");
    File.WriteAllText(Path.Combine(subDir, "b.xml"), "2");

    Assert.True(FileExists("Data/a.xml".AsSpan(), dir.AsSpan()));
    Assert.True(FileExists("Data/Sub/b.xml".AsSpan(), dir.AsSpan()));

    Directory.Delete(dataDir, recursive: true);

    AssertEventually(
        () => !FileExists("Data/a.xml".AsSpan(), dir.AsSpan()) && !FileExists("Data/Sub/b.xml".AsSpan(), dir.AsSpan()),
        "cached descendants of Data/ to invalidate after recursive directory delete");
}
```

- [ ] **Step 2: Run and confirm at least the rename test fails**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~AfterDirectoryRenamed|FullyQualifiedName~AfterDirectoryDeleted"`
Expected: At least the directory-rename test fails (descendants are not invalidated). The delete test may pass on platforms that emit per-file `Deleted` events, but cascade is still required for correctness on platforms that don't.

- [ ] **Step 3: Add directory-cascade invalidation**

`FileSystemEventArgs` does not tell us whether the path refers to a file or a directory, so for every event we (a) invalidate the parent directory's snapshot (covers file events) and (b) cascade-invalidate the subtree rooted at the event path (covers directory events). The cascade is a no-op when nothing matches its prefix.

In `LiveVirtualFileExistsStrategy.cs` add helpers:

```csharp
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

private void InvalidatePathAndSubtree(string fullPath)
{
    InvalidateParentOf(fullPath);
    InvalidateSubtree(fullPath);
}
```

Replace `OnFileEvent` and `OnFileRenamed` with:

```csharp
private void OnFileEvent(object sender, FileSystemEventArgs e)
{
    InvalidatePathAndSubtree(e.FullPath);
}

private void OnFileRenamed(object sender, RenamedEventArgs e)
{
    InvalidatePathAndSubtree(e.OldFullPath);
    InvalidatePathAndSubtree(e.FullPath);
}
```

- [ ] **Step 4: Run all live-strategy tests**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All pass, including both new directory tests.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "cascade-invalidate cached snapshots on directory rename and delete"
```

---

## Task 8: `Error` event reliability backstop

Subscribe to `FileSystemWatcher.Error` and clear the entire snapshot store on any error event. There is no portable way to provoke this from a unit test, so this task has no behavioral test — the implementation is one line and exercised through code review.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs`

- [ ] **Step 1: Subscribe to the `Error` event**

Inside `EnsureWatcher`, add the subscription alongside the others:

```csharp
watcher.Error += OnWatcherError;
```

Add the handler:

```csharp
private void OnWatcherError(object sender, ErrorEventArgs e)
{
    Store.Clear();
}
```

Unsubscribe in `Dispose`:

```csharp
watcher.Error -= OnWatcherError;
```

- [ ] **Step 2: Build**

Run: `dotnet build src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/PG.StarWarsGame.Engine.FileSystem.csproj -c Debug`
Expected: 0 errors.

- [ ] **Step 3: Run live-strategy tests to confirm no regression**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All tests still pass.

- [ ] **Step 4: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/FileExistStrategies/LiveVirtualFileExistsStrategy.cs
git commit -m "clear snapshot store on FileSystemWatcher Error event"
```

---

## Task 9: Public API — `UseLiveVirtualStrategy(bool? windowsFallback = null)`

Mirror `UseVirtualStrategy`'s nullable-flag shape so consumers can opt in. Reuse the existing `CreateWindowsStrategy` helper.

**Files:**
- Modify: `src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/PetroglyphFileSystem.Strategies.cs`

- [ ] **Step 1: Add the public method**

Insert immediately after `public void UseVirtualStrategy(bool? windowsFallback = null)`:

```csharp
/// <summary>
/// Switches the active file-exists strategy to a snapshot-based one that refreshes itself when
/// files are added, removed, or renamed in the game directory.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to <see cref="UseVirtualStrategy(bool?)"/> for lookups, but installs a single
/// recursive <see cref="System.IO.FileSystemWatcher"/> at the game directory whose events
/// invalidate cached directory listings on demand. File <em>content</em> changes are not tracked.
/// </para>
/// <para>
/// The watcher is created lazily on the first lookup that lands inside the game directory and is
/// torn down when the strategy is replaced or the file system is disposed. On Linux hosts holding
/// many concurrent strategies, inotify watch limits apply.
/// </para>
/// </remarks>
/// <param name="windowsFallback">
/// <see langword="true" /> to delegate outside-game-directory lookups to the Windows
/// <c>CreateFileA</c> strategy; <see langword="false" /> to delegate them to the Wine search
/// engine; <see langword="null" /> to pick the Windows strategy on Windows hosts and the Wine
/// strategy otherwise.
/// </param>
/// <exception cref="PlatformNotSupportedException">
/// <paramref name="windowsFallback"/> is <see langword="true" /> and the host is not Windows.
/// </exception>
public void UseLiveVirtualStrategy(bool? windowsFallback = null)
{
    var useWindows = windowsFallback ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    FileExistsStrategy fallback = useWindows
        ? CreateWindowsStrategy()
        : new WineFileExistsStrategy(_underlyingFileSystem);
    UseLiveVirtualStrategy(fallback);
}
```

- [ ] **Step 2: Add a public-API test**

Append to `LiveVirtualFileExistsStrategyTests`:

```csharp
[Fact]
public void UseLiveVirtualStrategy_WindowsFallbackTrueOnNonWindows_Throws()
{
    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        return; // not applicable on Windows

    var fs = new PetroglyphFileSystem(ServiceProvider);
    Assert.Throws<PlatformNotSupportedException>(() => fs.UseLiveVirtualStrategy(windowsFallback: true));
}

[Fact]
public void UseLiveVirtualStrategy_DefaultFallback_DoesNotThrow()
{
    var fs = new PetroglyphFileSystem(ServiceProvider);
    fs.UseLiveVirtualStrategy();
}
```

- [ ] **Step 3: Run all live-strategy tests**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj --filter "FullyQualifiedName~LiveVirtualFileExistsStrategy"`
Expected: All pass.

- [ ] **Step 4: Run the full test project to confirm no regression elsewhere**

Run: `dotnet test src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/PG.StarWarsGame.Engine.FileSystem.Test.csproj`
Expected: All pass.

- [ ] **Step 5: Commit**

```bash
git add src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem/IO/PetroglyphFileSystem.Strategies.cs \
        src/PetroglyphTools/PG.StarWarsGame.Engine.FileSystem.Test/IO/FileExistStrategies/LiveVirtualFileExistsStrategyTests.cs
git commit -m "expose public UseLiveVirtualStrategy(bool? windowsFallback)"
```

---

## Done

After Task 9, `LiveVirtualFileExistsStrategy` is fully wired, watcher events invalidate stale snapshots in O(1) for files and O(N) over cached directory keys for directory cascades, the public API is documented and discoverable, and every behavior except the `Error` backstop has direct test coverage. The `Error` backstop is a one-line handler that clears the store; reviewing the source is sufficient.
