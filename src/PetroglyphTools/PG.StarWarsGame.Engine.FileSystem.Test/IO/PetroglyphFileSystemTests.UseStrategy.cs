using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Testing.Attributes;
using PG.StarWarsGame.Engine.Utilities;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public class FileExistsStrategySwitchingTests : TestBaseWithPGFileSystem, IDisposable
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private readonly string _tempDir;

    public FileExistsStrategySwitchingTests()
    {
        _tempDir = FileSystem.Path.Combine(FileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());
        FileSystem.Directory.CreateDirectory(_tempDir);
        var filePath = FileSystem.Path.Combine(_tempDir, "test.txt");
        FileSystem.File.WriteAllText(filePath, "x");
    }

    protected override IFileSystem CreateFileSystem()
    {
        return new RealFileSystem();
    }

    public void Dispose()
    {
        try
        {
            if (FileSystem.Directory.Exists(_tempDir))
                FileSystem.Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            /* best-effort cleanup */
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void DefaultStrategy_ResolvesFilesAfterConstruction()
    {
        AssertExists();
    }

    [Fact]
    public void UseWineStrategy_Resolves()
    {
        PgFileSystem.UseWineStrategy();
        AssertExists();
    }

    [Fact]
    public void UseVirtualStrategy_DefaultFallback_Resolves()
    {
        PgFileSystem.UseVirtualStrategy();
        AssertExists();
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void UseWindowsStrategy_OnWindows_Resolves()
    {
        PgFileSystem.UseWindowsStrategy();
        AssertExists();
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void UseWindowsStrategy_OnNonWindows_Throws()
    {
        Assert.Throws<PlatformNotSupportedException>(() => PgFileSystem.UseWindowsStrategy());
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void UseVirtualStrategy_WindowsFallback_OnNonWindows_Throws()
    {
        Assert.Throws<PlatformNotSupportedException>(() => PgFileSystem.UseVirtualStrategy(windowsFallback: true));
    }

    [Fact]
    public void Switching_BetweenStrategies_LeavesFileSystemUsable()
    {
        PgFileSystem.UseWineStrategy();
        AssertExists();

        PgFileSystem.UseVirtualStrategy();
        AssertExists();

        if (IsWindows)
        {
            PgFileSystem.UseWindowsStrategy();
            AssertExists();
        }
    }

    [Fact]
    public void Switching_FromVirtual_DoesNotLeakStaleAnswers()
    {
        PgFileSystem.UseVirtualStrategy();
        AssertExists();

        var second = FileSystem.Path.Combine(_tempDir, "second.txt");
        FileSystem.File.WriteAllText(second, "y");

        PgFileSystem.UseWineStrategy();

        AssertExists("second.txt");
    }

    private void AssertExists(string file = "test.txt")
    {
        Assert.True(FileExists(file.AsSpan(), _tempDir.AsSpan()));
    }
}
