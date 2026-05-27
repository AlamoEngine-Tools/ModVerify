using System;
using System.IO;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class GameRepositoryFileLookupTests : EngineRepositoryTestBase
{
    [Fact]
    public void FileExists_FileInGameDir_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml"));
    }

    [Fact]
    public void FileExists_MissingFile_ReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Data/XML/DoesNotExist.xml"));
    }

    [Fact]
    public void FileExists_OutParams_FileInGameDir_NotInMeg()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
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
    public void FileExists_MegFileOnlyFlag_FilesystemHitIsIgnored()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "<x/>"))
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
            .WithGame(g => g.Write("Data/XML/Foo.xml", payload))
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
            .WithGame(g => g.Write("Data/XML/Foo.xml", "x"))
            .Build();
        var gameRepo = CreateRepository(repo);

        using var stream = gameRepo.TryOpenFile("Data/XML/Foo.xml");
        Assert.NotNull(stream);
    }

    [Fact]
    public void FileExists_ModOverridesGame()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "from-game"))
            .WithMod("MyMod", m => m.Write("Data/XML/Foo.xml", "from-mod"))
            .Build();
        var gameRepo = CreateRepository(repo);

        gameRepo.FileExists("Data/XML/Foo.xml", megFileOnly: false, out _, out var actualPath);
        Assert.NotNull(actualPath);
        Assert.Contains("MyMod", actualPath);

        Assert.Equal("from-mod", ReadAll(gameRepo.OpenFile("Data/XML/Foo.xml")));
    }

    [Fact]
    public void FileExists_ModPathOrderIsRespected()
    {
        using var repo = CreateBuilder()
            .WithMod("ModA", m => m.Write("Data/XML/Foo.xml", "from-A"))
            .WithMod("ModB", m => m.Write("Data/XML/Foo.xml", "from-B"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("from-A", ReadAll(gameRepo.OpenFile("Data/XML/Foo.xml")));
    }

    [Fact]
    public void FileExists_FallbackOnlyHit_FoundUnderDataPrefix()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("Data/XML/FromFallback.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/FromFallback.xml"));
    }

    [Fact]
    public void FileExists_NonDataPath_DoesNotConsultFallback()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("Other/Hidden.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Other/Hidden.xml"));
    }

    [Fact]
    public void FileExists_GameWinsOverFallback()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "from-game"))
            .WithFallbackGame(f => f.Write("Data/XML/Foo.xml", "from-fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("from-game", ReadAll(gameRepo.OpenFile("Data/XML/Foo.xml")));
    }

    [Fact]
    public void FileExists_ForwardSlashAndBackslash_BothResolve()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.xml", "x"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml"));
        Assert.True(gameRepo.FileExists(@"Data\XML\Foo.xml"));
    }

    [Fact]
    public void FileExists_DataPathWithDotPrefix_FallbackHit()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("Data/XML/Foo.xml", "fb"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("./Data/XML/Foo.xml"));
    }

    [Fact]
    public void FileExists_WithExtensionsSweep_FindsFirstMatch()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/XML/Foo.alo", "a"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/XML/Foo.xml", [".xml", ".alo"]));
        Assert.False(gameRepo.FileExists("Data/XML/Foo.xml", [".bar", ".baz"]));
    }

    [Fact]
    public void FileExists_OverlongPath_ReturnsFalseWithoutSurfacingTooLong()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        var path = new string('a', 300) + ".xml";

        var found = gameRepo.FileExists(path.AsSpan(), megFileOnly: false, out var pathTooLong);

        Assert.False(found);
        Assert.False(pathTooLong);
    }

    [Fact]
    public void Path_TrailingSeparator_ReflectsTopMostRoot()
    {
        using var repo = CreateBuilder()
            .WithMod("OnlyMod", _ => { })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), gameRepo.Path);
        Assert.Contains("OnlyMod", gameRepo.Path);
    }

    [Fact]
    public void Path_NoModConfigured_PointsToGameDirectory()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), gameRepo.Path);
        Assert.Equal(System.IO.Path.GetFullPath(repo.GameLocations.GamePath)
                     + System.IO.Path.DirectorySeparatorChar, gameRepo.Path);
    }
}
