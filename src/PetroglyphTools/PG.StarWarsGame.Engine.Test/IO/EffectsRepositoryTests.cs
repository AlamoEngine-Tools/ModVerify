using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Single-file existence checks against the EffectsRepository. Parameterized over equivalent
/// shader-name forms so that bare names, .fx-suffixed names, and arbitrary-suffixed names all
/// resolve identically (the engine strips any trailing extension before probing).
/// </summary>
public abstract class EffectsRepositoryTests : EngineRepositoryTestBase
{
    protected override RepositoryLookupSetup GetLookupSetup() => new(
        PopulateGame: g =>
        {
            g.Write("Data/Art/Shaders/MyShader.fx", "fs-fx");
            g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Art/Shaders/OtherShader.fx", "meg-fx"));
        },
        SelectRepository: gameRepo => gameRepo.EffectsRepository,
        FilesystemLookup: "MyShader",
        FilesystemContent: "fs-fx",
        MegLookup: "OtherShader",
        MegContent: "meg-fx");

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_OnlyFx_ResolvesUnderBare(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("MyShader.fx", "fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_OnlyFxo_FallsBackToFxo(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("MyShader.fxo", "fxo"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_OnlyFxh_FallsBackToFxh(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("MyShader.fxh", "fxh"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_FoundUnderShadersDirectory(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Shaders/MyShader.fx", "fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_FoundUnderShadersTerrain(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Shaders/Terrain/MyShader.fx", "fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_FoundUnderShadersEngine(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Shaders/Engine/MyShader.fx", "fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_MissingShader_ReturnsFalse(string input)
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FileExists_ShaderInMeg_Resolves(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("MyShader.fx", "fx-in-meg")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }
}
