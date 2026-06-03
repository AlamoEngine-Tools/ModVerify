using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class EffectsRepositoryTests
{
    #region Extension priority (outer loop)
    
    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Extension_FxBeatsOtherExtensions_SameLocation(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("MyShader.fx", "fx");
                g.Write("MyShader.fxo", "fxo");
                g.Write("MyShader.fxh", "fxh");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fx", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Extension_FxoBeatsFxh_SameLocation(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("MyShader.fxo", "fxo");
                g.Write("MyShader.fxh", "fxh");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fxo", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Extension_FxInDeepestDir_BeatsFxoAtBare(string input)
    {
        // Extension is the outer key in the lookup loop:
        // .fx is probed in every directory before .fxo is tried anywhere.
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/Art/Shaders/MyShader.fx", "fx-deepest");
                g.Write("MyShader.fxo", "fxo-bare");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fx-deepest", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Extension_FxInFallback_BeatsFxoInModSameDir(string input)
    {
        // Extension priority dominates over chain position:
        // .fx in fallback beats .fxo in a mod, because .fx is probed everywhere first.
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write("Data/Art/Shaders/MyShader.fxo", "mod-fxo"))
            .WithFallbackGame(f => f.Write("Data/Art/Shaders/MyShader.fx", "fb-fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fb-fx", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    #endregion

    #region Directory priority (middle loop)
    
    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Directory_BareBeatsShaders_SameExtension(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("MyShader.fx", "bare");
                g.Write("Data/Art/Shaders/MyShader.fx", "shaders");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("bare", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Directory_ShadersBeatsTerrain(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/Art/Shaders/MyShader.fx", "shaders");
                g.Write("Data/Art/Shaders/Terrain/MyShader.fx", "terrain");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("shaders", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    #endregion

    #region Root vs Data priority (inner loop)

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void RootLevel_ModIsInvisible_GameDirWins(string input)
    {
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write("MyShader.fx", "mod-root-unreachable"))
            .ConfigureGame(g => g.Write("MyShader.fx", "game-root"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-root", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void RootLevel_FallbackIsInvisible_MegWins(string input)
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("MyShader.fx", "fb-root-unreachable"))
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg", m => m.Add("MyShader.fx", "meg")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("meg", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void RootLevel_GameBeatsMeg(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("MyShader.fx", "game-root");
                g.WriteMeg("Data/Patch.meg", m => m.Add("MyShader.fx", "meg-root"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-root", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    #endregion

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FullMatrix_HighestReachablePriorityWins(string input)
    {
        using var repo = CreateBuilder()
            .WithMod("Mod", m =>
            {
                m.Write("Data/Art/Shaders/MyShader.fx", "WINNER");
                m.Write("Data/Art/Shaders/Terrain/MyShader.fx", "L1");
                m.Write("Data/Art/Shaders/MyShader.fxo", "L2");
            })
            .ConfigureGame(g =>
            {
                g.Write("Data/Art/Shaders/MyShader.fx", "L3");
                g.Write("Data/Art/Shaders/Engine/MyShader.fx", "L4");
                g.WriteMeg("Data/Patch.meg", meg =>
                {
                    meg.Add("Data/Art/Shaders/MyShader.fx", "L5");
                    meg.Add("Data/Art/Shaders/MyShader.fxo", "L6");
                });
            })
            .WithFallbackGame(f =>
            {
                f.Write("Data/Art/Shaders/MyShader.fx", "L7");
                f.Write("Data/Art/Shaders/MyShader.fxh", "L8");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("WINNER", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void GameRootFx_BeatsModShadersFx(string input)
    {
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write("Data/Art/Shaders/MyShader.fx", "mod-shaders"))
            .ConfigureGame(g => g.Write("MyShader.fx", "game-root"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-root", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void OnlyFxh_StillResolves(string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/Art/Shaders/MyShader.fxh", "fxh-deepest"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fxh-deepest", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }
}
