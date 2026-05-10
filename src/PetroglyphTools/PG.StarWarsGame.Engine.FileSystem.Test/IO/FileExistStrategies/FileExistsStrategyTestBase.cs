using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

public abstract class FileExistsStrategyTestBase : TestBaseWithPGFileSystem, IDisposable
{
    private readonly List<string> _tempDirs = [];

    protected override IFileSystem CreateFileSystem()
    {
        return new RealFileSystem();
    }

    protected abstract override void ConfigureStrategy(PetroglyphFileSystem fs);

    protected virtual void AssertResolvedPath(string expectedOnDiskPath, string actualResult)
    {
        var expected = expectedOnDiskPath.Replace('\\', FileSystem.Path.DirectorySeparatorChar).Replace('/', FileSystem.Path.DirectorySeparatorChar);
        var actual = actualResult.Replace('\\', FileSystem.Path.DirectorySeparatorChar).Replace('/', FileSystem.Path.DirectorySeparatorChar);
        var ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        Assert.Equal(expected, actual, ignoreCase: ignoreCase);
    }

    public virtual void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            try
            {
                if (FileSystem.Directory.Exists(dir))
                    FileSystem.Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // Ignore
            }
        }
        GC.SuppressFinalize(this);
    }

    protected string NewTempDir()
    {
        var dir = FileSystem.Path.Combine(FileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());
        FileSystem.Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    // ---------------------------------------------------------------------------------------------
    // Shared tests — every strategy must satisfy.
    // ---------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("/gameDir")]
    [InlineData(null)]
    public void FileExists_ExistingFullyQualifiedFile_ReturnsTrue(string? gameDir)
    {
        var dir = NewTempDir();
        var file = FileSystem.Path.Combine(dir, "tmp.dat");
        FileSystem.File.WriteAllText(file, "x");

        var fullGameDir = gameDir is null ? null : FileSystem.Path.GetFullPath(gameDir);

        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists(file.AsSpan(), ref sb, fullGameDir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Theory]
    [InlineData("/gameDir")]
    [InlineData(null)]
    public void FileExists_NonExistingFullyQualifiedFile_ReturnsFalse(string? gameDir)
    {
        var missing = FileSystem.Path.Combine(FileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());

        var fullGameDir = gameDir is null ? null : FileSystem.Path.GetFullPath(gameDir);

        var exists = FileExists(missing.AsSpan(), fullGameDir.AsSpan());

        Assert.False(exists);
    }

    [Fact]
    public void FileExists_RelativePathUnderGameDirectory_ReturnsTrue()
    {
        var dir = NewTempDir();
        var file = FileSystem.Path.Combine(dir, "test.txt");
        FileSystem.File.WriteAllText(file, "x");

        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists("test.txt".AsSpan(), ref sb, dir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Fact]
    public void FileExists_GameDirectoryWithTrailingSeparator_ReturnsTrue()
    {
        var dir = NewTempDir();
        var dirWithTrailing = dir + FileSystem.Path.DirectorySeparatorChar;
        var file = FileSystem.Path.Combine(dir, "test.txt");
        FileSystem.File.WriteAllText(file, "x");

        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists("test.txt".AsSpan(), ref sb, dirWithTrailing.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Fact]
    public void FileExists_MissingIntermediateDirectory_ReturnsFalse()
    {
        var dir = NewTempDir();
        // Create dir/a/c.txt — note: no "b" intermediate.
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "a"));
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dir, "a", "c.txt"), "x");

        var exists = FileExists("a/b/c.txt".AsSpan(), dir.AsSpan());

        Assert.False(exists);
    }

    [Fact]
    public void FileExists_FullyQualifiedPathOutsideGameDirectory_ReturnsTrue()
    {
        var root = NewTempDir();
        var gameDir = FileSystem.Path.Combine(root, "game");
        var otherDir = FileSystem.Path.Combine(root, "other", "DATA");
        FileSystem.Directory.CreateDirectory(gameDir);
        FileSystem.Directory.CreateDirectory(otherDir);
        var file = FileSystem.Path.Combine(otherDir, "FILE.TXT");
        FileSystem.File.WriteAllText(file, "x");

        // Pass a fully-qualified path with mismatched casing for the leaf; gameDir is unrelated.
        var input = FileSystem.Path.Combine(FileSystem.Path.GetDirectoryName(otherDir)!, "data", "file.txt");

        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists(input.AsSpan(), ref sb, gameDir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Fact]
    public void FileExists_DotSegmentInInputPath_ReturnsTrue()
    {
        var dir = NewTempDir();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "DATA"));
        var file = FileSystem.Path.Combine(dir, "DATA", "FILE.TXT");
        FileSystem.File.WriteAllText(file, "x");

        // Leading "./" plus mismatched casing — the dispatcher must normalize the dot away.
        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists(@".\DATA\file.txt".AsSpan(), ref sb, dir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Fact]
    public void FileExists_DotDotSegmentInInputPath_ReturnsTrue()
    {
        var dir = NewTempDir();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "DATA"));
        var file = FileSystem.Path.Combine(dir, "DATA", "FILE.TXT");
        FileSystem.File.WriteAllText(file, "x");

        // Other/.. cancels out so the dispatcher must resolve to dir/DATA/file.txt.
        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists(@"Other\..\DATA\file.txt".AsSpan(), ref sb, dir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(file, sb.ToString());
    }

    [Theory]
    // Resolves to the game directory itself.
    [InlineData(".")]
    [InlineData("./")]
    [InlineData(@".\")]
    // Resolves to a subdirectory.
    [InlineData("Data")]
    [InlineData("Data/")]
    [InlineData(@"Data\")]
    [InlineData("Data/.")]
    [InlineData("Data/./")]
    [InlineData(@"Data\.\")]
    // Resolves to a deeper subdirectory (with case-folded variant).
    [InlineData("Data/foo")]
    [InlineData("DATA/FOO")]
    public void FileExists_PathResolvesToDirectory_ReturnsFalse(string input)
    {
        var dir = NewTempDir();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "Data", "foo"));

        var exists = FileExists(input.AsSpan(), dir.AsSpan());

        Assert.False(exists);
    }

    [Fact]
    public void FileExists_EmptyRelativePath_ReturnsFalse()
    {
        // After joining "" with the base directory, the dispatcher hands the strategy the bare
        // base directory. Every strategy must treat that as not-a-file rather than reporting the
        // directory itself as existing.
        var dir = NewTempDir();
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(dir, "x.txt"), "x");

        Assert.False(FileExists(string.Empty.AsSpan(), dir.AsSpan()));
    }

    [Theory]
    [InlineData("test.txt", "TEST.txt")]
    [InlineData("dir/test.txt", "DIR/TEST.txt")]
    [InlineData("a/b/c.txt", "A/B/C.txt")]
    [InlineData("A/B/C.txt", "a/b/c.txt")]
    [InlineData("a/B/c.txt", "A/b/C.txt")]
    [InlineData("a/B/C.txt", "a/B/c.txt")]
    [InlineData("a/b/C/D.txt", "a/b/c/d.txt")]
    public void FileExists_AnyCasing_ReturnsTrue(string inputPath, string actualPathOnDisk)
    {
        var dir = NewTempDir();
        var fullOnDisk = FileSystem.Path.Combine(dir, actualPathOnDisk.Replace('/', FileSystem.Path.DirectorySeparatorChar));
        var fullOnDiskParent = FileSystem.Path.GetDirectoryName(fullOnDisk);
        if (fullOnDiskParent != null)
            FileSystem.Directory.CreateDirectory(fullOnDiskParent);
        FileSystem.File.WriteAllText(fullOnDisk, "x");

        var sb = new ValueStringBuilder();
        var exists = PgFileSystem.FileExists(inputPath.AsSpan(), ref sb, dir.AsSpan());

        Assert.True(exists);
        AssertResolvedPath(fullOnDisk, sb.ToString());
    }
}
