using System;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class TextureRepositoryTests : EngineRepositoryTestBase
{
    protected override RepositoryLookupSetup GetLookupSetup() => new(
        PopulateGame: g =>
        {
            g.Write("Data/Art/Textures/MyTex.tga", "fs-tga");
            g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Art/Textures/OtherTex.tga", "meg-tga"));
        },
        SelectRepository: gameRepo => gameRepo.TextureRepository,
        FilesystemLookup: "Data/Art/Textures/MyTex.tga",
        FilesystemContent: "fs-tga",
        MegLookup: "Data/Art/Textures/OtherTex.tga",
        MegContent: "meg-tga");

    [Fact]
    public void FileExists_AsIs_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Textures/MyTex.tga", "tga"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.TextureRepository.FileExists("Data/Art/Textures/MyTex.tga"));
    }

    [Fact]
    public void FileExists_DdsFallback_OnlyDdsExists()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Textures/MyTex.dds", "dds"))
            .Build();
        var gameRepo = CreateRepository(repo);

        // Engine asks for .tga; .dds is what's on disk.
        Assert.True(gameRepo.TextureRepository.FileExists("Data/Art/Textures/MyTex.tga"));
    }

    [Fact]
    public void FileExists_BareName_FoundUnderTexturesDirectory()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Textures/MyTex.tga", "tga"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.TextureRepository.FileExists("MyTex.tga"));
    }

    [Fact]
    public void FileExists_BareNameDdsFallback_FoundUnderTexturesDirectory()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Textures/MyTex.dds", "dds"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.TextureRepository.FileExists("MyTex.tga"));
    }

    [Fact]
    public void FileExists_Missing_ReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.TextureRepository.FileExists("Missing.tga"));
    }

    [Fact]
    public void FileExists_TextureInMeg_Resolves()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Art/Textures/MyTex.tga", "tga")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.TextureRepository.FileExists("Data/Art/Textures/MyTex.tga"));
    }

    [Fact]
    public void FileExists_OverlongPath_FlagsPathTooLong()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        var path = new string('a', 300) + ".tga";
        var found = gameRepo.TextureRepository.FileExists(path.AsSpan(), megFileOnly: false, out var pathTooLong);

        Assert.False(found);
        Assert.True(pathTooLong);
    }
}
