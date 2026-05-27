using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Verifies the EffectsRepository lookup priority when the same shader could be resolved from multiple locations.
/// Priority (outermost → innermost): extension (.fx → .fxo → .fxh), then directory (bare → SHADERS → SHADERS\TERRAIN → SHADERS\ENGINE),
/// then file-lookup chain (Mod → Game → MEG → Fallback).
/// Every test is parameterized over the input shader-name forms (with/without extensions) — the engine strips
/// the trailing extension, so all forms must resolve identically.
/// </summary>
public abstract class EffectsRepositoryPriorityTests : EngineRepositoryTestBase
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

    // ----------------------- extension priority (outer loop) -----------------------

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Extension_FxBeatsFxo_SameLocation(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("MyShader.fx", "fx");
                g.Write("MyShader.fxo", "fxo");
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
            .WithGame(g =>
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
            .WithGame(g =>
            {
                g.Write("Data/Art/Shaders/Engine/MyShader.fx", "fx-deepest");
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

    // ----------------------- directory priority (middle loop) -----------------------

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Directory_BareBeatsShaders_SameExtension(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
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
            .WithGame(g =>
            {
                g.Write("Data/Art/Shaders/MyShader.fx", "shaders");
                g.Write("Data/Art/Shaders/Terrain/MyShader.fx", "terrain");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("shaders", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Directory_TerrainBeatsEngine(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("Data/Art/Shaders/Terrain/MyShader.fx", "terrain");
                g.Write("Data/Art/Shaders/Engine/MyShader.fx", "engine");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("terrain", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    // ----------------------- chain priority (innermost, per probe) -----------------------

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Chain_ModBeatsGame_SameExtAndDir(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Shaders/MyShader.fx", "game"))
            .WithMod("Mod", m => m.Write("Data/Art/Shaders/MyShader.fx", "mod"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("mod", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Chain_GameBeatsMeg_SameExtAndDir(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("Data/Art/Shaders/MyShader.fx", "game");
                g.WriteMeg("Data/Patch.meg", m => m.Add("Data/Art/Shaders/MyShader.fx", "meg"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Chain_MegBeatsFallback_SameExtAndDir(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                m => m.Add("Data/Art/Shaders/MyShader.fx", "meg")))
            .WithFallbackGame(f => f.Write("Data/Art/Shaders/MyShader.fx", "fallback"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("meg", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void Chain_ModOrderIsRespected(string input)
    {
        using var repo = CreateBuilder()
            .WithMod("ModA", m => m.Write("Data/Art/Shaders/MyShader.fx", "A"))
            .WithMod("ModB", m => m.Write("Data/Art/Shaders/MyShader.fx", "B"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("A", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    // ----------------------- bare-path quirk: mods/fallback are bypassed -----------------------

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void BareLevel_ModIsInvisible_GameDirWins(string input)
    {
        // A bare path (no "Data/" prefix) does not flow through the mod-path or fallback-path branches
        // (FileFromAltExists short-circuits on non-DATA paths). Only game-dir + master MEG can serve.
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write("MyShader.fx", "mod-bare-unreachable"))
            .WithGame(g => g.Write("MyShader.fx", "game-bare"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-bare", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void BareLevel_FallbackIsInvisible_MegWins(string input)
    {
        // Same quirk on the fallback side: fallback is only consulted for DATA paths,
        // so a bare-path probe falls through to the master MEG.
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.Write("MyShader.fx", "fb-bare-unreachable"))
            .WithGame(g => g.WriteMeg("Data/Patch.meg", m => m.Add("MyShader.fx", "meg")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("meg", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void BareLevel_GameBeatsMeg(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("MyShader.fx", "game-bare");
                g.WriteMeg("Data/Patch.meg", m => m.Add("MyShader.fx", "meg-bare"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-bare", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    // ----------------------- combined matrix -----------------------

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void FullMatrix_HighestReachablePriorityWins(string input)
    {
        // No one has anything at the bare-path level, so the first reachable probe is
        // .fx in SHADERS through the full chain → mod wins.
        using var repo = CreateBuilder()
            .WithMod("Mod", m =>
            {
                m.Write("Data/Art/Shaders/MyShader.fx", "WINNER");
                m.Write("Data/Art/Shaders/Terrain/MyShader.fx", "L1");
                m.Write("Data/Art/Shaders/MyShader.fxo", "L2");
            })
            .WithGame(g =>
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
    public void GameBareFx_BeatsModShadersFx(string input)
    {
        // Game-dir .fx at the bare path is the very first reachable probe, and it beats
        // the next-most-specific reachable file (.fx in mod's SHADERS).
        // Demonstrates: directory priority (bare > SHADERS) dominates chain priority (mod > game).
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write("Data/Art/Shaders/MyShader.fx", "mod-shaders"))
            .WithGame(g => g.Write("MyShader.fx", "game-bare"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("game-bare", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }

    [Theory]
    [MemberData(nameof(ShaderTestData.Inputs), MemberType = typeof(ShaderTestData))]
    public void OnlyFxh_StillResolves(string input)
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Shaders/Engine/MyShader.fxh", "fxh-deepest"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("fxh-deepest", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
    }
}
