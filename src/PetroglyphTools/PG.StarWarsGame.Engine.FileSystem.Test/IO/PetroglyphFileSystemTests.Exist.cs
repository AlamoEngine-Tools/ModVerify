using System;
using System.IO;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Testing.Attributes;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    [Fact]
    public void FileExists_EmptyGameDirectory_Works()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(tempFile.AsSpan(), ref vsb, ReadOnlySpan<char>.Empty);
            Assert.True(exists);
            Assert.Equal(tempFile, vsb.ToString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void FileExists_FileExists()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(tempFile.AsSpan(), ref vsb, string.Empty.AsSpan());
            Assert.True(exists);
            Assert.Equal(tempFile, vsb.ToString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void FileExists_FileDoesNotExist()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var vsb = new ValueStringBuilder();
        var exists = _pgFileSystem.FileExists(tempFile.AsSpan(), ref vsb, string.Empty.AsSpan());
        Assert.False(exists);
    }

    [Fact]
    public void FileExists_RelativePath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(tempFile, "test");
        try
        {
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists("test.txt".AsSpan(), ref vsb, tempDir.AsSpan());
            Assert.True(exists);
            
            // On Windows, JoinPath might use backslashes. 
            // PetroglyphFileSystem.JoinPath uses _underlyingFileSystem.Path.DirectorySeparatorChar if no separator is present.
            // Since _fileSystem is RealFileSystem, it will be \ on Windows.
            var expected = Path.Combine(tempDir, "test.txt");
            Assert.Equal(expected, vsb.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Theory]
    [InlineData("test.txt", "TEST.txt")]
    [InlineData("dir/test.txt", "DIR/TEST.txt")]
    [InlineData("a/b/c.txt", "A/B/C.txt")]
    [InlineData("A/B/C.txt", "a/b/c.txt")]
    [InlineData("a/B/c.txt", "A/b/C.txt")]
    [InlineData("a/B/C.txt", "a/B/c.txt")]
    [InlineData("a/b/C/D.txt", "a/b/c/d.txt")]
    public void FileExists_CaseInsensitive(string inputPath, string actualPathOnDisk)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var fullPathOnDisk = Path.Combine(tempDir, actualPathOnDisk.Replace('/', Path.DirectorySeparatorChar));
        var fullPathOnDiskDir = Path.GetDirectoryName(fullPathOnDisk);
        if (fullPathOnDiskDir != null)
            Directory.CreateDirectory(fullPathOnDiskDir);
        
        File.WriteAllText(fullPathOnDisk, "test");
        
        try
        {
            var vsb = new ValueStringBuilder();
            // On Windows, CreateFile is case-insensitive by default.
            // On Linux, FileExistsCaseInsensitive handles it.
            var exists = _pgFileSystem.FileExists(inputPath.AsSpan(), ref vsb, tempDir.AsSpan());
            Assert.True(exists);
            
            var resultPath = vsb.ToString();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows CreateFile doesn't change the path in the string builder to the actual case-sensitive path if it just found it.
                // It stays as what was passed to it (with gameDirectory joined).
                var expected = _fileSystem.Path.Combine(tempDir, inputPath);
                Assert.Equal(expected, resultPath);
            }
            else
            {
                // On Linux, FileExistsCaseInsensitive DOES update the string builder:
                // stringBuilder.Length = 0;
                // stringBuilder.Append(file);
                // It should be the exact path on disk.
                Assert.Equal(fullPathOnDisk, resultPath);
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FileExists_GameDirectory_WithTrailingSeparator()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(tempFile, "test");
        try
        {
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists("test.txt".AsSpan(), ref vsb, tempDir.AsSpan());
            Assert.True(exists);
            Assert.Equal(tempFile, vsb.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_DotSegmentInPath_ReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create the actual file at tempDir/DATA/FILE.TXT (uppercase)
            Directory.CreateDirectory(Path.Combine(tempDir, "DATA"));
            File.WriteAllText(Path.Combine(tempDir, "DATA", "FILE.TXT"), "test");

            // Input path uses a leading ".\" (dot-segment) AND different casing.
            // After normalization + join: tempDir/./DATA/file.txt
            // File.Exists fast-path fails (case mismatch), so the impl must resolve case-insensitively.
            // Correct impls must handle "." as a valid path segment that resolves to the current directory,
            // not treat it as a literal directory name to look up via GetDirectories.
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(@".\DATA\file.txt".AsSpan(), ref vsb, tempDir.AsSpan());

            Assert.True(exists);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_MissingIntermediateDirectory_ReturnsFalse()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create tempDir/a/c.txt — no "b" directory at all
            Directory.CreateDirectory(Path.Combine(tempDir, "a"));
            File.WriteAllText(Path.Combine(tempDir, "a", "c.txt"), "test");

            // Input path references a non-existent intermediate segment "b"
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists("a/b/c.txt".AsSpan(), ref vsb, tempDir.AsSpan());

            Assert.False(exists);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Theory]
#if Windows
    [InlineData("C:\\test.txt", true)]
    [InlineData("/test.txt", false)] // On Windows, /test.txt is NOT fully qualified (it's root-relative to current drive)
    [InlineData("\\test.txt", false)] 
#else
    [InlineData("/test.txt", true)]
    [InlineData("C:\\test.txt", false)] // On Linux, C:\ is not a root
#endif
    [InlineData("test.txt", false)]
    public void IsPathFullyQualified_Exists_Internal(string path, bool expected)
    {
        // This method is internal/private, but we can indirectly test it through FileExists or use reflection if we want to be explicit.
        // Actually, FileExists calls it.
        // If it's fully qualified, it doesn't join with gameDirectory.
        
        var gameDir = "Z:\\non-existent-dir";
        var vsb = new ValueStringBuilder();
        _pgFileSystem.FileExists(path.AsSpan(), ref vsb, gameDir.AsSpan());
        
        var resultPath = vsb.ToString().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var expectedGameDir = gameDir.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        if (expected)
        {
            Assert.StartsWith(path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar), resultPath);
            Assert.DoesNotContain(expectedGameDir, resultPath);
        }
        else
        {
            Assert.Contains(expectedGameDir, resultPath);
        }
    }
}
