using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class GameRepositoryTests
{
    private const string TextEntry = "Data/Text/text.txt";

    #region Intra-origin: several MEGs in the same game

    [Fact]
    public void MegaFilesXml_ThreeListedMegs_LastListedWins()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/A.meg</File>
              <File>Data/B.meg</File>
              <File>Data/C.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/A.meg", m => m.Add(TextEntry, "A"));
                g.WriteMeg("Data/B.meg", m => m.Add(TextEntry, "B"));
                g.WriteMeg("Data/C.meg", m => m.Add(TextEntry, "C"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("C", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_PatchSlotsOverrideMegaFilesXml_64PatchWinsOverall()
    {
        // Full precedence for one entry present in every slot: MegaFiles.xml-listed < Patch < Patch2 < 64Patch.
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/Custom.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/Custom.meg",  m => m.Add(TextEntry, "Custom"));
                g.WriteMeg("Data/Patch.meg",   m => m.Add(TextEntry, "Patch"));
                g.WriteMeg("Data/Patch2.meg",  m => m.Add(TextEntry, "Patch2"));
                g.WriteMeg("Data/64Patch.meg", m => m.Add(TextEntry, "64Patch"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("64Patch", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }

    #endregion

    #region Inter-origin: mod MEGs shadow game MEGs

    [Fact]
    public void ModPatchMeg_ShadowsGamePatchMeg()
    {
        // A patch slot is resolved through the file-lookup chain, so the mod's Data/Patch.meg is loaded
        // instead of the game's — the game's copy never reaches the master MEG.
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg", m => m.Add(TextEntry, "game")))
            .WithMod("Mod", m => m.WriteMeg("Data/Patch.meg", meg => meg.Add(TextEntry, "mod")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("mod", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }

    [Fact]
    public void ModMegaFilesXml_ShadowsGameMegaFilesXml()
    {
        // Data/MegaFiles.xml is itself resolved through the chain, so only the mod's list is read; a MEG
        // listed solely in the game's MegaFiles.xml is never loaded.
        const string modXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/ModListed.meg</File>
            </MegaFiles>
            """;
        const string gameXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/GameListed.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", gameXml);
                g.WriteMeg("Data/GameListed.meg", m => m.Add("Data/Text/game.txt", "game"));
            })
            .WithMod("Mod", m =>
            {
                m.Write("Data/MegaFiles.xml", modXml);
                m.WriteMeg("Data/ModListed.meg", meg => meg.Add("Data/Text/mod.txt", "mod"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("mod", ReadAll(gameRepo.OpenFile("Data/Text/mod.txt", megFileOnly: true)));
        Assert.False(gameRepo.FileExists("Data/Text/game.txt", megFileOnly: true));
    }

    [Fact]
    public void MegaFilesXml_MixesModAndGameMegs_LaterListedWins()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/FromMod.meg</File>
              <File>Data/FromGame.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/FromGame.meg", m => m.Add(TextEntry, "game"));
            })
            .WithMod("Mod", m => m.WriteMeg("Data/FromMod.meg", meg => meg.Add(TextEntry, "mod")))
            .Build();
        var gameRepo = CreateRepository(repo);

        // FromGame.meg is listed after FromMod.meg, so the game entry wins for the shared path.
        Assert.Equal("game", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }

    #endregion

    // Both of these normalized paths hash to CRC32 0x2AAF63A4:
    //   model      : DATA\ART\MODELS\MOV_EMPIRE_INTRO_SHUTTLE_FIRE_DIE_00.ALA
    //   WAV entry  : U000_EMP0212_ENG.WAV
    // The model request collides with the bare WAV entry once the engine has prefixed the model directory,
    // which is why the lookup is driven through DATA\ART\MODELS below.
    private const string ModelLookup = @"Data\Art\Models\MOV_EMPIRE_INTRO_SHUTTLE_FIRE_DIE_00.ALA";
    private const string CollidingWav = "U000_EMP0212_ENG.WAV";

    [Fact]
    public void Crc32Collision_ModelLoadedFirst_ShadowedByLaterWav()
    {
        // Both files genuinely exist in the master MEG. The model's MEG (First.meg) is loaded before the
        // WAV's MEG (Second.meg), so the later WAV takes over the shared CRC32 slot.
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.RegisterAndWriteMeg("Data/First.meg", meg => meg.Add(ModelLookup, "model-bytes"));
                g.RegisterAndWriteMeg("Data/Second.meg", meg => meg.Add(CollidingWav, "wav-bytes"));
            })
            .Build();
        var modelRepo = CreateRepository(repo).ModelRepository;

        var found = modelRepo.FileExists(ModelLookup, megFileOnly: false, out var inMeg, out var actualFilePath);

        // The model exists, yet requesting it resolves to the colliding WAV that was loaded later: the
        // engine reports the WAV's path and hands back the WAV's bytes instead of the model.
        Assert.True(found);
        Assert.True(inMeg);
        Assert.Equal(CollidingWav, actualFilePath);
        Assert.Equal("wav-bytes", ReadAll(modelRepo.OpenFile(ModelLookup)));
    }
}
