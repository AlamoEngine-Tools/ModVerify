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
            // Correct implementations must handle "." as a valid path segment that resolves to the current directory,
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
    public void FileExists_CaseInsensitive_DotDotSegmentInPath_ReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create tempDir/DATA/FILE.TXT
            Directory.CreateDirectory(Path.Combine(tempDir, "DATA"));
            File.WriteAllText(Path.Combine(tempDir, "DATA", "FILE.TXT"), "test");

            // Input path uses ".." to go up from a sibling directory.
            // After join + normalization: tempDir/Other/../DATA/file.txt
            // The implementation must resolve ".." by popping the previous component.
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(@"Other\..\DATA\file.txt".AsSpan(), ref vsb, tempDir.AsSpan());

            Assert.True(exists);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_FullyQualifiedPathOutsideGameDirectory_ReturnsTrue()
    {
        // Scenario where gameDirectory is /a/b/c but actualFilePath is /x/y/foo.txt
        // The file path is fully qualified and shares no common prefix with gameDirectory beyond "/".
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gameDir = Path.Combine(tempDir, "game");
        var otherDir = Path.Combine(tempDir, "other");

        Directory.CreateDirectory(gameDir);
        Directory.CreateDirectory(Path.Combine(otherDir, "DATA"));

        try
        {
            // File lives completely outside the game directory
            var fileOnDisk = Path.Combine(otherDir, "DATA", "FILE.TXT");
            File.WriteAllText(fileOnDisk, "test");

            // Pass a fully qualified path with wrong casing; gameDir is unrelated
            var fullyQualifiedInput = Path.Combine(otherDir, "data", "file.txt");
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(fullyQualifiedInput.AsSpan(), ref vsb, gameDir.AsSpan());

            Assert.True(exists);
            Assert.Equal(fileOnDisk, vsb.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_FullyQualifiedSiblingPath_ReturnsTrue()
    {
        // Scenario where gameDirectory is /a/b/c but actualFilePath is /a/b/x/foo.txt
        // They share a common parent but diverge before the game directory ends.
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gameDir = Path.Combine(tempDir, "game");
        var siblingDir = Path.Combine(tempDir, "sibling");

        Directory.CreateDirectory(gameDir);
        Directory.CreateDirectory(Path.Combine(siblingDir, "DATA"));

        try
        {
            var fileOnDisk = Path.Combine(siblingDir, "DATA", "FILE.TXT");
            File.WriteAllText(fileOnDisk, "test");

            // Fully qualified path into sibling directory with wrong casing
            var fullyQualifiedInput = Path.Combine(siblingDir, "data", "file.txt");
            var vsb = new ValueStringBuilder();
            var exists = _pgFileSystem.FileExists(fullyQualifiedInput.AsSpan(), ref vsb, gameDir.AsSpan());

            Assert.True(exists);
            Assert.Equal(fileOnDisk, vsb.ToString());
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

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_RepeatedCallsSameDirectory_BothResolve()
    {
        // Verifies the directory cache: second call to a sibling file in the same on-disk dir
        // must still return true with correct casing — even though the cache short-circuits the walk.
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var dataDir = Path.Combine(tempDir, "Mods", "Test", "Data", "Xml");
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(dataDir, "foo.xml"), "1");
            File.WriteAllText(Path.Combine(dataDir, "bar.xml"), "2");

            var vsb1 = new ValueStringBuilder();
            var first = _pgFileSystem.FileExists("MODS/TEST/DATA/XML/FOO.XML".AsSpan(), ref vsb1, tempDir.AsSpan());
            Assert.True(first);
            Assert.Equal(Path.Combine(dataDir, "foo.xml"), vsb1.ToString());

            // Second call — different file in the same dir, also wrong casing.
            // Should hit the dir cache populated by the first call.
            var vsb2 = new ValueStringBuilder();
            var second = _pgFileSystem.FileExists("mods/test/data/xml/BAR.XML".AsSpan(), ref vsb2, tempDir.AsSpan());
            Assert.True(second);
            Assert.Equal(Path.Combine(dataDir, "bar.xml"), vsb2.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_NegativeCacheForMissingIntermediate_StillReturnsFalse()
    {
        // After a miss caches a non-existent intermediate directory, a second call (different leaf)
        // through the same missing dir must still return false — and not erroneously claim the file.
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "Mods", "Test", "Data", "Xml"));
            File.WriteAllText(Path.Combine(tempDir, "Mods", "Test", "Data", "Xml", "foo.xml"), "1");

            // First call: "Other" doesn't exist as a sibling of "Xml". Caches negative entry.
            var vsb1 = new ValueStringBuilder();
            var first = _pgFileSystem.FileExists("MODS/TEST/DATA/OTHER/foo.xml".AsSpan(), ref vsb1, tempDir.AsSpan());
            Assert.False(first);

            // Second call: same missing directory, different leaf. Negative cache hit.
            var vsb2 = new ValueStringBuilder();
            var second = _pgFileSystem.FileExists("mods/test/data/other/bar.xml".AsSpan(), ref vsb2, tempDir.AsSpan());
            Assert.False(second);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_FastPathHandlesDotSegmentsAfterCachePopulated()
    {
        // After the cache is populated by a clean-path call, a follow-up call with dot segments
        // pointing at the same resolved directory must still hit the fast path correctly.
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var dataDir = Path.Combine(tempDir, "Mods", "Test", "Data", "Xml");
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(dataDir, "foo.xml"), "1");
            File.WriteAllText(Path.Combine(dataDir, "bar.xml"), "2");

            // Prime the cache.
            var vsb1 = new ValueStringBuilder();
            Assert.True(_pgFileSystem.FileExists("MODS/TEST/DATA/XML/FOO.XML".AsSpan(), ref vsb1, tempDir.AsSpan()));

            // Same resolved parent, but the input has dot segments. Fast path must resolve dots
            // before computing the cache key.
            var vsb2 = new ValueStringBuilder();
            var second = _pgFileSystem.FileExists(@"Other\..\MODS\TEST\DATA\XML\BAR.XML".AsSpan(), ref vsb2, tempDir.AsSpan());
            Assert.True(second);
            Assert.Equal(Path.Combine(dataDir, "bar.xml"), vsb2.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void FileExists_CaseInsensitive_SiblingDirectoryAfterCachedParent_Resolves()
    {
        // After a successful call populates ancestor entries, a sibling directory under a
        // cached parent must resolve correctly (the walk skips cached prefixes).
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var xmlDir = Path.Combine(tempDir, "Mods", "Test", "Data", "Xml");
            var artDir = Path.Combine(tempDir, "Mods", "Test", "Data", "Art");
            Directory.CreateDirectory(xmlDir);
            Directory.CreateDirectory(artDir);
            File.WriteAllText(Path.Combine(xmlDir, "foo.xml"), "1");
            File.WriteAllText(Path.Combine(artDir, "bar.dds"), "2");

            // Populate cache for /tempDir/Mods/Test/Data and below.
            var vsb1 = new ValueStringBuilder();
            Assert.True(_pgFileSystem.FileExists("MODS/TEST/DATA/XML/FOO.XML".AsSpan(), ref vsb1, tempDir.AsSpan()));

            // Sibling directory "Art" under the now-cached "/Mods/Test/Data".
            var vsb2 = new ValueStringBuilder();
            var second = _pgFileSystem.FileExists("MODS/TEST/DATA/ART/BAR.DDS".AsSpan(), ref vsb2, tempDir.AsSpan());
            Assert.True(second);
            Assert.Equal(Path.Combine(artDir, "bar.dds"), vsb2.ToString());
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
        // This method is internal/private, but we can indirectly test it through FileExists

        const string gameDir = "Z:\\non-existent-dir";
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
