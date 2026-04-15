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
    public void HasTrailingDirectorySeparator(string path, bool expected)
    {
        Assert.Equal(expected, _pgFileSystem.HasTrailingDirectorySeparator(path.AsSpan()));
    }
}
