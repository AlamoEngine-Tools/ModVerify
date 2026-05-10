using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    private readonly IFileSystem _fileSystem;
    private readonly PetroglyphFileSystem _pgFileSystem;

    public PetroglyphFileSystemTests()
    {
        _fileSystem = new RealFileSystem();
        var sc = new ServiceCollection();
        sc.AddSingleton(_fileSystem);
        IServiceProvider serviceProvider = sc.BuildServiceProvider();
        _pgFileSystem = new PetroglyphFileSystem(serviceProvider);
    }

    [Fact]
    public void Ctor_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PetroglyphFileSystem(null!));
    }

    [Fact]
    public void UnderlyingFileSystem_ReturnsCorrectInstance()
    {
        Assert.Same(_fileSystem, _pgFileSystem.UnderlyingFileSystem);
    }

    [Theory]
    [InlineData("dir/", true)]
    [InlineData("dir\\", true)]
    [InlineData("dir/file.txt", false)]
    [InlineData("file.txt", false)]
    [InlineData("", false)]
    [InlineData("/", true)]
    [InlineData("\\", true)]
    [InlineData("a", false)]
    [InlineData(".", false)]
    [InlineData("..", false)]
    [InlineData("dir//", true)]
    [InlineData("dir\\\\", true)]
    public void HasTrailingDirectorySeparator(string path, bool expected)
    {
        Assert.Equal(expected, _pgFileSystem.HasTrailingDirectorySeparator(path.AsSpan()));
    }

    [Fact]
    public void OpenRead_ExistingFile_ReturnsReadableStream()
    {
        var dir = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());
        _fileSystem.Directory.CreateDirectory(dir);
        try
        {
            var file = _fileSystem.Path.Combine(dir, "openread.bin");
            _fileSystem.File.WriteAllBytes(file, new byte[] { 1, 2, 3, 4 });

            using var stream = _pgFileSystem.OpenRead(file);

            Assert.True(stream.CanRead);
            Assert.False(stream.CanWrite);
            Assert.Equal(4, stream.Length);
            var buf = new byte[4];
            Assert.Equal(4, stream.Read(buf, 0, 4));
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, buf);
        }
        finally
        {
            _fileSystem.Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void OpenRead_MissingFile_Throws()
    {
        var missing = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid() + ".missing");
        Assert.ThrowsAny<System.IO.IOException>(() => _pgFileSystem.OpenRead(missing));
    }
}
