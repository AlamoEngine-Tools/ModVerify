using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
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

public abstract class LiveVirtualFileExistsStrategyTests : VirtualFileExistsStrategyBaseTests
{
    /// <summary>
    /// Hard cap on how long we'll wait for the OS to deliver a watcher event. The OS delivers
    /// events asynchronously; we poll the cache state at <see cref="PollInterval"/> until the
    /// expected condition holds, only failing if the deadline passes.
    /// </summary>
    private static readonly TimeSpan WatcherEventTimeout = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    private protected override void ConfigureStrategy(PetroglyphFileSystem fs, FileExistsStrategy underlying)
        => fs.UseLiveVirtualStrategy(underlying);

    private protected override FileExistsStrategy CreateStrategyForDisposeTest()
        => new LiveVirtualFileExistsStrategy(FileSystem, new WineFileExistsStrategy(FileSystem));

    [Fact]
    public async Task FileExists_AfterFileDeletedOnDisk_ReportsMissing()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        var file = FileSystem.Path.Combine(dataDir, "foo.xml");
        FileSystem.File.WriteAllText(file, "x");

        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));

        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(file),
            () => !FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()),
            "snapshot to refresh after Data/foo.xml was deleted on disk");
    }

    [Fact]
    public async Task FileExists_AfterFileCreatedOnDisk_ReportsPresent()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "seed.xml"), "x");

        // Prime the snapshot.
        Assert.True(FileExists("Data/seed.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/new.xml".AsSpan(), dir.AsSpan()));

        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "new.xml"), "y"),
            () => FileExists("Data/new.xml".AsSpan(), dir.AsSpan()),
            "snapshot to refresh after Data/new.xml was created on disk");
    }

    [Fact]
    public async Task FileExists_AfterFileRenamed_OldNameMissingNewNamePresent()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        var oldPath = FileSystem.Path.Combine(dataDir, "old.xml");
        var newPath = FileSystem.Path.Combine(dataDir, "new.xml");
        FileSystem.File.WriteAllText(oldPath, "x");

        Assert.True(FileExists("Data/old.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/new.xml".AsSpan(), dir.AsSpan()));

        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Move(oldPath, newPath),
            () => !FileExists("Data/old.xml".AsSpan(), dir.AsSpan())
                  && FileExists("Data/new.xml".AsSpan(), dir.AsSpan()),
            "snapshot to reflect the rename of Data/old.xml to Data/new.xml");
    }

    [Fact]
    public async Task FileExists_AfterDirectoryRenamed_OldPathMissingNewPathPresent()
    {
        var dir = NewTempDir();
        var oldDir = FileSystem.Path.Combine(dir, "OldData");
        var newDir = FileSystem.Path.Combine(dir, "NewData");
        FileSystem.Directory.CreateDirectory(oldDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(oldDir, "foo.xml"), "x");

        Assert.True(FileExists("OldData/foo.xml".AsSpan(), dir.AsSpan()));

        await AwaitCacheInvalidationAsync(
            () => FileSystem.Directory.Move(oldDir, newDir),
            () => !FileExists("OldData/foo.xml".AsSpan(), dir.AsSpan())
                  && FileExists("NewData/foo.xml".AsSpan(), dir.AsSpan()),
            "cached descendants of OldData to invalidate after directory rename");
    }

    [Fact]
    public async Task FileExists_AfterDirectoryDeleted_AllDescendantsInvalidated()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        var subDir = FileSystem.Path.Combine(dataDir, "Sub");
        FileSystem.Directory.CreateDirectory(subDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "a.xml"), "1");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(subDir, "b.xml"), "2");

        Assert.True(FileExists("Data/a.xml".AsSpan(), dir.AsSpan()));
        Assert.True(FileExists("Data/Sub/b.xml".AsSpan(), dir.AsSpan()));

        await AwaitCacheInvalidationAsync(
            () => FileSystem.Directory.Delete(dataDir, recursive: true),
            () => !FileExists("Data/a.xml".AsSpan(), dir.AsSpan())
                  && !FileExists("Data/Sub/b.xml".AsSpan(), dir.AsSpan()),
            "cached descendants of Data/ to invalidate after recursive directory delete");
    }

    [Fact]
    public void SwapStrategy_LiveThenWineThenLive_FreshUnderlyingHandlesOutOfBaseLookups()
    {
        var root = NewTempDir();
        var gameDir = FileSystem.Path.Combine(root, "gameDir");
        var outsideDir = FileSystem.Path.Combine(root, "outside");
        FileSystem.Directory.CreateDirectory(gameDir);
        FileSystem.Directory.CreateDirectory(outsideDir);
        var insideFile = FileSystem.Path.Combine(gameDir, "in.xml");
        var outsideFile = FileSystem.Path.Combine(outsideDir, "out.xml");
        FileSystem.File.WriteAllText(insideFile, "i");
        FileSystem.File.WriteAllText(outsideFile, "o");

        // First Live, with trackingA as the out-of-base fallback.
        var trackingA = new TrackingFileExistsStrategy(FileSystem) { ReturnValue = true, ResolvedPath = outsideFile };
        PgFileSystem.UseLiveVirtualStrategy(trackingA);

        Assert.True(FileExists("in.xml".AsSpan(), gameDir.AsSpan()));     // snapshot path, no delegation
        Assert.True(FileExists(outsideFile.AsSpan(), gameDir.AsSpan()));  // out-of-base → trackingA
        Assert.Equal(1, trackingA.CallCount);

        // Swap to Wine. SwapStrategy disposes the previous Live, which also disposes trackingA.
        PgFileSystem.UseWineStrategy();

        // Swap back to Live with a brand-new tracking underlying.
        var trackingB = new TrackingFileExistsStrategy(FileSystem) { ReturnValue = true, ResolvedPath = outsideFile };
        PgFileSystem.UseLiveVirtualStrategy(trackingB);

        // Out-of-base lookup must be routed through the NEW underlying. The old trackingA must
        // not be touched anymore — this is the assertion that catches stale references.
        Assert.True(FileExists(outsideFile.AsSpan(), gameDir.AsSpan()));
        Assert.Equal(1, trackingA.CallCount);
        Assert.Equal(1, trackingB.CallCount);

        // And the second Live owns its own snapshot store, so in-base lookups still bypass the
        // underlying.
        Assert.True(FileExists("in.xml".AsSpan(), gameDir.AsSpan()));
        Assert.Equal(1, trackingB.CallCount);
    }

    [Fact]
    public async Task FileExists_TracksMultipleBaseDirectoriesIndependently()
    {
        var root = NewTempDir();
        var gameDir = FileSystem.Path.Combine(root, "gameDir");
        var workshopDir = FileSystem.Path.Combine(root, "workshops", "myMod");
        FileSystem.Directory.CreateDirectory(gameDir);
        FileSystem.Directory.CreateDirectory(workshopDir);
        var gameFile = FileSystem.Path.Combine(gameDir, "game.xml");
        var workshopFile = FileSystem.Path.Combine(workshopDir, "mod.xml");
        FileSystem.File.WriteAllText(gameFile, "x");
        FileSystem.File.WriteAllText(workshopFile, "y");

        // Prime watchers for both base directories.
        Assert.True(FileExists("game.xml".AsSpan(), gameDir.AsSpan()));
        Assert.True(FileExists("mod.xml".AsSpan(), workshopDir.AsSpan()));

        // A change under the gameDir base must invalidate the gameDir snapshot…
        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(gameFile),
            () => !FileExists("game.xml".AsSpan(), gameDir.AsSpan()),
            "gameDir snapshot to refresh after game.xml deleted");

        // …but the workshop snapshot must still be live and serve mod.xml unchanged.
        Assert.True(FileExists("mod.xml".AsSpan(), workshopDir.AsSpan()));

        // And the converse — deleting under workshopDir must update only that base's snapshot.
        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(workshopFile),
            () => !FileExists("mod.xml".AsSpan(), workshopDir.AsSpan()),
            "workshopDir snapshot to refresh after mod.xml deleted");
    }

    [Fact]
    public async Task FileExists_NewDirectoryUnderTrackedBase_FirstLookupSnapshotsThenCacheServes()
    {
        var dir = NewTempDir();
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dir, "seed.xml"), "x");

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        ConfigureStrategy(PgFileSystem, tracking);

        // Prime the watcher on the base directory.
        Assert.True(FileExists("seed.xml".AsSpan(), dir.AsSpan()));

        // Create a new directory + file under the watched base after the watcher is up.
        var newDir = FileSystem.Path.Combine(dir, "NewDir");
        FileSystem.Directory.CreateDirectory(newDir);
        var newFile = FileSystem.Path.Combine(newDir, "foo.xml");
        FileSystem.File.WriteAllText(newFile, "y");

        // Wait for the watcher to invalidate the base directory's cache after the create.
        await AwaitCacheInvalidationAsync(
            () => { /* disk action already done */ },
            () => FileExists("NewDir/foo.xml".AsSpan(), dir.AsSpan()),
            "first lookup of NewDir/foo.xml to succeed against the freshly-snapshotted directory");

        var afterFirstLookup = tracking.CallCount;

        // Second lookup — the snapshot for NewDir is now in the store, so this is a cache hit.
        // Neither the underlying tracking strategy nor the disk should be re-consulted.
        Assert.True(FileExists("NewDir/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.Equal(afterFirstLookup, tracking.CallCount);

        // Underlying must never be called for in-base-dir paths regardless of lookup count.
        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public async Task WatcherError_BrokenWatcher_StrategyRecoversAndKeepsTrackingOnNextLookup()
    {
        // There's no portable way to make a real FileSystemWatcher fire Error (buffer overflow
        // is flaky/slow; root deletion is OS-dependent), so we synthesize the Error path by
        // invoking the strategy's private handler with the live watcher as sender. We do not
        // assert any internal state — only that the strategy keeps doing its job: lookups still
        // resolve, and subsequent disk changes are still picked up via a re-armed watcher.
        var baseDir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(baseDir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        var file = FileSystem.Path.Combine(dataDir, "foo.xml");
        FileSystem.File.WriteAllText(file, "x");

        // Prime: the live strategy installs a watcher and snapshots the directory.
        Assert.True(FileExists("Data/foo.xml".AsSpan(), baseDir.AsSpan()));

        var strategy = GetActiveLiveStrategy();
        InvokeOnWatcherError(strategy, GetWatchers(strategy)[baseDir], new ErrorEventArgs(new IOException("simulated")));

        // 1) Lookups still resolve correctly after the Error.
        Assert.True(FileExists("Data/foo.xml".AsSpan(), baseDir.AsSpan()));

        // 2) The strategy keeps tracking: a subsequent disk change is still reflected.
        //    (Implicitly verifies the next lookup re-armed a working watcher.)
        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(file),
            () => !FileExists("Data/foo.xml".AsSpan(), baseDir.AsSpan()),
            "snapshot to invalidate after Data/foo.xml deleted (post-Error rebuild)");
    }

    [Fact]
    public async Task WatcherError_OneOfManyRoots_OtherRootStillTracksChanges()
    {
        // An Error on one root must not impair the strategy's ability to track changes
        // under unrelated roots, nor prevent the broken root from recovering on next use.
        var root = NewTempDir();
        var gameDir = FileSystem.Path.Combine(root, "gameDir");
        var workshopDir = FileSystem.Path.Combine(root, "workshops", "myMod");
        FileSystem.Directory.CreateDirectory(gameDir);
        FileSystem.Directory.CreateDirectory(workshopDir);
        var gameFile = FileSystem.Path.Combine(gameDir, "g.xml");
        var workshopFile = FileSystem.Path.Combine(workshopDir, "m.xml");
        FileSystem.File.WriteAllText(gameFile, "g");
        FileSystem.File.WriteAllText(workshopFile, "m");

        // Prime both bases.
        Assert.True(FileExists("g.xml".AsSpan(), gameDir.AsSpan()));
        Assert.True(FileExists("m.xml".AsSpan(), workshopDir.AsSpan()));

        // Synthesize an Error on the gameDir watcher only.
        var strategy = GetActiveLiveStrategy();
        InvokeOnWatcherError(strategy, GetWatchers(strategy)[gameDir], new ErrorEventArgs(new IOException("simulated")));

        // 1) The workshop watcher is still live — deleting a file there invalidates its cache.
        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(workshopFile),
            () => !FileExists("m.xml".AsSpan(), workshopDir.AsSpan()),
            "workshop snapshot to invalidate after m.xml deleted; gameDir Error must not affect it");

        // 2) The broken root still serves lookups (next call rebuilds snapshot + re-arms watcher).
        Assert.True(FileExists("g.xml".AsSpan(), gameDir.AsSpan()));

        // 3) After re-arm, the gameDir watcher tracks changes again.
        await AwaitCacheInvalidationAsync(
            () => FileSystem.File.Delete(gameFile),
            () => !FileExists("g.xml".AsSpan(), gameDir.AsSpan()),
            "gameDir snapshot to invalidate after g.xml deleted (post-Error rebuild)");
    }

    private LiveVirtualFileExistsStrategy GetActiveLiveStrategy()
    {
        var field = typeof(PetroglyphFileSystem).GetField("_strategy", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (LiveVirtualFileExistsStrategy)field.GetValue(PgFileSystem)!;
    }

    // GetWatchers / InvokeOnWatcherError exist only to *synthesize* an Error event (no portable
    // way to make a real FSW fire one). The Error tests themselves assert observable behavior,
    // not the watcher dictionary's contents.
    private static Dictionary<string, IFileSystemWatcher> GetWatchers(LiveVirtualFileExistsStrategy strategy)
    {
        var field = typeof(LiveVirtualFileExistsStrategy).GetField("_watchers", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Dictionary<string, IFileSystemWatcher>)field.GetValue(strategy)!;
    }

    private static void InvokeOnWatcherError(LiveVirtualFileExistsStrategy strategy, IFileSystemWatcher sender, ErrorEventArgs args)
    {
        var method = typeof(LiveVirtualFileExistsStrategy).GetMethod("OnWatcherError", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(strategy, [sender, args]);
    }

    protected static async Task AwaitCacheInvalidationAsync(Action diskAction, Func<bool> predicate, string description)
    {
        diskAction();

        var ct = TestContext.Current.CancellationToken;
        var sw = Stopwatch.StartNew();
        while (true)
        {
            if (predicate())
                return;
            if (sw.Elapsed >= WatcherEventTimeout)
                Assert.Fail($"Timed out after {WatcherEventTimeout} waiting for: {description}");
            await Task.Delay(PollInterval, ct);
        }
    }
}
