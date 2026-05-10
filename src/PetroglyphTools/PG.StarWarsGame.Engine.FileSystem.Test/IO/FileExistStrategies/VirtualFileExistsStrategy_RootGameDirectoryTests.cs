using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Testing.Attributes;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

/// <summary>
/// Exercises the snapshot path with the filesystem root (<c>/</c>) as the game directory.
/// Real-disk fixtures cannot create files at <c>/</c> without root privileges, so this uses
/// a Linux-simulated <see cref="MockFileSystem"/>. Only meaningful for the non-live variant —
/// the live variant's <see cref="System.IO.FileSystemWatcher"/> binds to the real OS, not the mock.
/// </summary>
public sealed class VirtualFileExistsStrategy_RootGameDirectoryTests
{
    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_GameDirectoryIsFilesystemRoot_ResolvesFromSnapshot()
    {
        var mockFs = new MockFileSystem();
        mockFs.File.WriteAllText("/foo.xml", "x");

        var pgFs = NewPgFs(mockFs);
        var tracking = new TrackingFileExistsStrategy(mockFs);
        pgFs.UseVirtualStrategy(tracking);

        var sb = new ValueStringBuilder();
        Assert.True(pgFs.FileExists("/foo.xml".AsSpan(), ref sb, "/".AsSpan()));
        Assert.Equal("/foo.xml", sb.ToString());

        // Lookup is under the game directory, so it must resolve from the snapshot, not delegate.
        Assert.Equal(0, tracking.CallCount);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_GameDirectoryIsFilesystemRoot_MissingFile_ReportsFalseWithoutDelegating()
    {
        var mockFs = new MockFileSystem();
        mockFs.File.WriteAllText("/foo.xml", "x");

        var pgFs = NewPgFs(mockFs);
        var tracking = new TrackingFileExistsStrategy(mockFs);
        pgFs.UseVirtualStrategy(tracking);

        var sb = new ValueStringBuilder();
        Assert.False(pgFs.FileExists("/missing.xml".AsSpan(), ref sb, "/".AsSpan()));
        Assert.Equal(0, tracking.CallCount);
    }

    private static PetroglyphFileSystem NewPgFs(IFileSystem fileSystem)
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(fileSystem);
        return new PetroglyphFileSystem(sc.BuildServiceProvider());
    }
}
