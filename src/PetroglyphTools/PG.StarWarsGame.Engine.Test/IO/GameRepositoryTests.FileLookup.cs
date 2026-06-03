using System.IO;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class GameRepositoryTests
{
    [Fact]
    public void FileExists_MissingFile_ReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Data/XML/DoesNotExist.xml"));
    }

    [Fact]
    public void FileExists_FileInGameDir_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
            .ConfigureGame(g => g.WriteXml("Bar.xml", "<y/>"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml"));
        Assert.True(gameRepo.FileExists("Data/XML/Bar.xml"));
    }

    [Fact]
    public void FileExists_OutParams_FileInGameDir_NotInMeg()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
            .Build();
        var gameRepo = CreateRepository(repo);

        var found = gameRepo.FileExists("Data/XML/Foo.xml", megFileOnly: false, out var inMeg, out var actualPath);

        Assert.True(found);
        Assert.False(inMeg);
        Assert.NotNull(actualPath);
        Assert.EndsWith("Foo.xml", actualPath);
    }

    [Fact]
    public void FileExists_OutParams_MissingFile_ActualPathIsNull()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        var found = gameRepo.FileExists("Data/XML/Missing.xml", megFileOnly: false, out _, out var actualPath);

        Assert.False(found);
        Assert.Null(actualPath);
    }

    [Fact]
    public void FileExists_MegFileOnlyFlag_FileSystemHitIsIgnored()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml"));
        Assert.False(gameRepo.FileExists("Data/XML/Foo.xml", megFileOnly: true));
    }

    [Fact]
    public void OpenFile_FileInGameDir_ReturnsStreamWithContent()
    {
        const string payload = "hello-world";
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", payload))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(payload, ReadAll(gameRepo.OpenFile("Data/XML/Foo.xml")));
    }

    [Fact]
    public void OpenFile_Missing_ThrowsFileNotFound()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.Throws<FileNotFoundException>(() => gameRepo.OpenFile("Data/XML/Missing.xml"));
    }

    [Fact]
    public void TryOpenFile_Missing_ReturnsNull()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.Null(gameRepo.TryOpenFile("Data/XML/Missing.xml"));
    }

    [Fact]
    public void TryOpenFile_Present_ReturnsStream()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "x"))
            .Build();
        var gameRepo = CreateRepository(repo);

        using var stream = gameRepo.TryOpenFile("Data/XML/Foo.xml");
        Assert.NotNull(stream);
        Assert.Equal("x", ReadAll(stream));
    }

    [Fact]
    public void FileExists_ModOverridesGame()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "from-game"))
            .WithMod("MyMod", m => m.Write("Data/XML/Foo.xml", "from-mod"))
            .Build();
        var gameRepo = CreateRepository(repo);

        gameRepo.FileExists("Data/XML/Foo.xml", megFileOnly: false, out _, out var actualPath);
        Assert.NotNull(actualPath);
        Assert.Contains("MyMod", actualPath);

        Assert.Equal("from-mod", ReadAll(gameRepo.OpenFile("Data/XML/Foo.xml")));
    }

    [Fact]
    public void FileExists_NonDataPath_DoesNotConsultModOrFallback()
    {
        using var repo = CreateBuilder()
            .WithMod("MyMod", f => f.Write("Other/Hidden.xml", "mod"))
            .WithFallbackGame(f => f.Write("Other/Hidden.xml", "fbg"))
            .WithFallback("Fallback", f => f.Write("Other/Hidden.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Other/Hidden.xml"));
    }

    [Fact]
    public void FileExists_NonDataPath_TakeFromGame()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(f => f.Write("Other/Foo.xml", "game"))
            .WithMod("MyMod", f => f.Write("Other/Foo.xml", "mod"))
            .WithFallbackGame(f => f.Write("Other/Foo.xml", "fbg"))
            .WithFallback("Fallback", f => f.Write("Other/Foo.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Other/Foo.xml"));
        Assert.Equal("game", ReadAll(gameRepo.OpenFile("Other/Foo.xml")));

    }

    [Fact]
    public void FileExists_ForwardSlashAndBackslash_BothResolve()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/XML/Foo.xml", "x"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml"));
        Assert.True(gameRepo.FileExists("Data\\XML\\Foo.xml"));
        Assert.True(gameRepo.FileExists("Data\\XML/Foo.xml"));
        Assert.True(gameRepo.FileExists("Data/XML\\Foo.xml"));
    }

    [Fact]
    public void FileExists_DataPathWithDotPrefix_ModHit()
    {
        using var repo = CreateBuilder()
            .WithMod("MyMod", f => f.Write("Data/XML/Foo.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);
        
        Assert.True(gameRepo.FileExists("./Data/XML/Foo.xml"));
        Assert.True(gameRepo.FileExists(".\\Data\\XML\\Foo.xml"));
        Assert.True(gameRepo.FileExists("./Data\\XML\\Foo.xml"));
        Assert.True(gameRepo.FileExists(".\\Data/XML\\Foo.xml"));
    }

    [Fact]
    public void FileExists_DataPathWithDotPrefix_FallbackHit()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("Data/XML/Foo.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("./Data/XML/Foo.xml"));
        Assert.True(gameRepo.FileExists(".\\Data\\XML\\Foo.xml"));
        Assert.True(gameRepo.FileExists("./Data\\XML\\Foo.xml"));
        Assert.True(gameRepo.FileExists(".\\Data/XML\\Foo.xml"));
    }

    [Fact]
    public void EmptyRepository_NoErrors_LookupReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("anything.txt"));
        Assert.Null(gameRepo.TryOpenFile("anything.txt"));
    }
}
