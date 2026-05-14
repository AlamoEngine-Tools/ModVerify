using System;
using System.Collections.Concurrent;
using System.Reflection;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

/// <summary>
/// Tests every <see cref="VirtualFileExistsStrategyBase"/>-derived strategy must satisfy:
/// per-directory snapshotting, no delegation for in-tree paths, delegation for out-of-tree
/// paths, and missing-directory handling.
/// </summary>
public abstract class VirtualFileExistsStrategyBaseTests : FileExistsStrategyTestBase
{
    /// <summary>
    /// Switch the active strategy on <paramref name="fs"/> to the strategy under test, with
    /// <paramref name="underlying"/> as the fallback for outside-game-directory lookups.
    /// </summary>
    private protected abstract void ConfigureStrategy(PetroglyphFileSystem fs, FileExistsStrategy underlying);

    [Fact]
    public void FileExists_RepeatedCallsSameDirectory_BothResolveFromSnapshot()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Mods", "Test", "Data", "Xml");
        FileSystem.Directory.CreateDirectory(dataDir);
        var foo = FileSystem.Path.Combine(dataDir, "foo.xml");
        var bar = FileSystem.Path.Combine(dataDir, "bar.xml");
        FileSystem.File.WriteAllText(foo, "1");
        FileSystem.File.WriteAllText(bar, "2");

        var sb1 = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists("MODS/TEST/DATA/XML/FOO.XML".AsSpan(), ref sb1, dir.AsSpan()));
        AssertResolvedPath(foo, sb1.ToString());

        var sb2 = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists("mods/test/data/xml/BAR.XML".AsSpan(), ref sb2, dir.AsSpan()));
        AssertResolvedPath(bar, sb2.ToString());
    }

    [Fact]
    public void FileExists_MissingDirectoryUnderGameRoot_RemainsMissing()
    {
        var dir = NewTempDir();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "Mods", "Test", "Data", "Xml"));
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dir, "Mods", "Test", "Data", "Xml", "foo.xml"), "1");

        Assert.False(FileExists("MODS/TEST/DATA/OTHER/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("mods/test/data/other/bar.xml".AsSpan(), dir.AsSpan()));
    }

    [Fact]
    public void FileExists_PathOutsideGameDirectory_DelegatesToUnderlying()
    {
        var root = NewTempDir();
        var gameDir = FileSystem.Path.Combine(root, "game");
        var outsideDir = FileSystem.Path.Combine(root, "outside");
        FileSystem.Directory.CreateDirectory(gameDir);
        FileSystem.Directory.CreateDirectory(outsideDir);
        var file = FileSystem.Path.Combine(outsideDir, "FILE.TXT");
        FileSystem.File.WriteAllText(file, "x");

        var tracking = new TrackingFileExistsStrategy(FileSystem) { ReturnValue = true, ResolvedPath = file };
        ConfigureStrategy(PgFileSystem, tracking);

        var sb = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists(file.AsSpan(), ref sb, gameDir.AsSpan()));
        AssertResolvedPath(file, sb.ToString());

        Assert.Equal(1, tracking.CallCount);
    }

    [Fact]
    public void FileExists_PathUnderGameDirectory_DoesNotDelegate()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "foo.xml"), "x");

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        ConfigureStrategy(PgFileSystem, tracking);

        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public void FileExists_RepeatedLookupInSnapshottedDirectory_DoesNotDelegate()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "foo.xml"), "x");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "bar.xml"), "y");

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        ConfigureStrategy(PgFileSystem, tracking);

        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.True(FileExists("Data/bar.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/missing.xml".AsSpan(), dir.AsSpan()));

        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public void FileExists_MissingSubdirectoryUnderGameRoot_DoesNotDelegate()
    {
        var dir = NewTempDir();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "Data"));

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        ConfigureStrategy(PgFileSystem, tracking);

        Assert.False(FileExists("Data/Other/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/Other/bar.xml".AsSpan(), dir.AsSpan()));

        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public void Cleanup_ClearsSnapshotCache_FreshSnapshotOnNextLookup()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "foo.xml"), "x");

        // Prime the snapshot — foo.xml is in cache.
        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.NotEmpty(GetSnapshotStore());

        // Cleanup evicts the snapshot cache.
        GetActiveVirtualStrategy().Cleanup();
        Assert.Empty(GetSnapshotStore());

        // Add a file to disk after cleanup; the post-cleanup re-snapshot must pick it up.
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dataDir, "bar.xml"), "y");

        // Post-cleanup: fresh snapshot taken on next lookup — both files visible.
        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.True(FileExists("Data/bar.xml".AsSpan(), dir.AsSpan()));
    }

    private VirtualFileExistsStrategyBase GetActiveVirtualStrategy()
    {
        var field = typeof(PetroglyphFileSystem).GetField("_strategy", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (VirtualFileExistsStrategyBase)field.GetValue(PgFileSystem)!;
    }

    private ConcurrentDictionary<string, VirtualDirectory?> GetSnapshotStore()
    {
        var strategy = GetActiveVirtualStrategy();
        var field = typeof(VirtualFileExistsStrategyBase).GetField("Store", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (ConcurrentDictionary<string, VirtualDirectory?>)field.GetValue(strategy)!;
    }
}
