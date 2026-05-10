using System;
using System.IO;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

#if Windows
public sealed class VirtualFileExistsStrateg_Windows : VirtualFileExistsStrategyTests
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
    {
        fs.UseVirtualStrategy(windowsFallback: true);
    }
}
#endif

public sealed class VirtualFileExistsStrategy_Wine : VirtualFileExistsStrategyTests
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
        => fs.UseVirtualStrategy();
}

public abstract class VirtualFileExistsStrategyTests : FileExistsStrategyTestBase
{
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

    [Fact]
    public void FileExists_MissingDirectoryUnderGameRoot_RemainsMissing()
    {
        var dir = NewTempDir();
        Directory.CreateDirectory(Path.Combine(dir, "Mods", "Test", "Data", "Xml"));
        File.WriteAllText(Path.Combine(dir, "Mods", "Test", "Data", "Xml", "foo.xml"), "1");

        Assert.False(FileExists("MODS/TEST/DATA/OTHER/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("mods/test/data/other/bar.xml".AsSpan(), dir.AsSpan()));
    }

    [Fact]
    public void FileExists_AfterFirstResolve_SnapshotServesSubsequentLookups()
    {
        var dir = NewTempDir();
        var dataDir = Path.Combine(dir, "Data");
        Directory.CreateDirectory(dataDir);
        var file = Path.Combine(dataDir, "foo.xml");
        File.WriteAllText(file, "x");

        Assert.True(FileExists("DATA/foo.xml".AsSpan(), dir.AsSpan()));

        File.Delete(file);

        Assert.True(FileExists("DATA/foo.xml".AsSpan(), dir.AsSpan()));
    }

    [Fact]
    public void FileExists_PathOutsideGameDirectory_DelegatesToUnderlying()
    {
        var root = NewTempDir();
        var gameDir = Path.Combine(root, "game");
        var outsideDir = Path.Combine(root, "outside");
        Directory.CreateDirectory(gameDir);
        Directory.CreateDirectory(outsideDir);
        var file = Path.Combine(outsideDir, "FILE.TXT");
        File.WriteAllText(file, "x");

        var tracking = new TrackingFileExistsStrategy(FileSystem) { ReturnValue = true, ResolvedPath = file };
        PgFileSystem.UseVirtualStrategy(tracking);

        var sb = new ValueStringBuilder();
        Assert.True(PgFileSystem.FileExists(file.AsSpan(), ref sb, gameDir.AsSpan()));
        AssertResolvedPath(file, sb.ToString());

        Assert.Equal(1, tracking.CallCount);
    }

    [Fact]
    public void FileExists_PathUnderGameDirectory_DoesNotDelegate()
    {
        var dir = NewTempDir();
        var dataDir = Path.Combine(dir, "Data");
        Directory.CreateDirectory(dataDir);
        File.WriteAllText(Path.Combine(dataDir, "foo.xml"), "x");

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        PgFileSystem.UseVirtualStrategy(tracking);

        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public void FileExists_RepeatedLookupInSnapshottedDirectory_DoesNotDelegate()
    {
        var dir = NewTempDir();
        var dataDir = Path.Combine(dir, "Data");
        Directory.CreateDirectory(dataDir);
        File.WriteAllText(Path.Combine(dataDir, "foo.xml"), "x");
        File.WriteAllText(Path.Combine(dataDir, "bar.xml"), "y");

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        PgFileSystem.UseVirtualStrategy(tracking);

        Assert.True(FileExists("Data/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.True(FileExists("Data/bar.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/missing.xml".AsSpan(), dir.AsSpan()));

        Assert.Equal(0, tracking.CallCount);
    }

    [Fact]
    public void FileExists_MissingSubdirectoryUnderGameRoot_DoesNotDelegate()
    {
        var dir = NewTempDir();
        Directory.CreateDirectory(Path.Combine(dir, "Data"));

        var tracking = new TrackingFileExistsStrategy(FileSystem);
        PgFileSystem.UseVirtualStrategy(tracking);

        Assert.False(FileExists("Data/Other/foo.xml".AsSpan(), dir.AsSpan()));
        Assert.False(FileExists("Data/Other/bar.xml".AsSpan(), dir.AsSpan()));

        Assert.Equal(0, tracking.CallCount);
    }
}
