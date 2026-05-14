using System;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
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

public abstract class VirtualFileExistsStrategyTests : VirtualFileExistsStrategyBaseTests
{
    private protected override void ConfigureStrategy(PetroglyphFileSystem fs, FileExistsStrategy underlying)
        => fs.UseVirtualStrategy(underlying);

    private protected override FileExistsStrategy CreateStrategyForCleanupTest()
        => new VirtualFileExistsStrategy(FileSystem, new WineFileExistsStrategy(FileSystem));

    [Fact]
    public void FileExists_AfterFirstResolve_SnapshotServesSubsequentLookups()
    {
        var dir = NewTempDir();
        var dataDir = FileSystem.Path.Combine(dir, "Data");
        FileSystem.Directory.CreateDirectory(dataDir);
        var file = FileSystem.Path.Combine(dataDir, "foo.xml");
        FileSystem.File.WriteAllText(file, "x");

        Assert.True(FileExists("DATA/foo.xml".AsSpan(), dir.AsSpan()));

        FileSystem.File.Delete(file);

        // Non-live strategy: snapshot is taken once and serves all subsequent lookups even if
        // the file is deleted on disk. The live variant overrides this behavior.
        Assert.True(FileExists("DATA/foo.xml".AsSpan(), dir.AsSpan()));
    }
}
