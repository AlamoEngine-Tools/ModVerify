using System.Linq;
using PG.StarWarsGame.Engine.ErrorReporting;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO.Foc;

public partial class FocGameRepositoryTests
{
    [Fact]
    public void Init_LoadsEawFallbackPatchMegs()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f =>
            {
                f.WriteMeg("Data/Patch.meg",   m => m.Add("Init/EawPatch.bin",   "EawPatch"));
                f.WriteMeg("Data/Patch2.meg",  m => m.Add("Init/EawPatch2.bin",  "EawPatch2"));
                f.WriteMeg("Data/64Patch.meg", m => m.Add("Init/Eaw64Patch.bin", "Eaw64Patch"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("EawPatch",   ReadAll(gameRepo.OpenFile("Init/EawPatch.bin",   megFileOnly: true)));
        Assert.Equal("EawPatch2",  ReadAll(gameRepo.OpenFile("Init/EawPatch2.bin",  megFileOnly: true)));
        Assert.Equal("Eaw64Patch", ReadAll(gameRepo.OpenFile("Init/Eaw64Patch.bin", megFileOnly: true)));
    }

    [Fact]
    public void Init_LoadsEawFallbackMegaFilesXmlMegs()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/EawCustom.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .WithFallbackGame(f =>
            {
                f.Write("Data/MegaFiles.xml", megaFilesXml);
                f.WriteMeg("Data/EawCustom.meg", m => m.Add("Init/InEawCustom.bin", "EawCustom"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("EawCustom", ReadAll(gameRepo.OpenFile("Init/InEawCustom.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_EawPatch2OverridesEawPatch()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(f =>
            {
                f.WriteMeg("Data/Patch.meg",  m => m.Add("Init/EawConflict.bin", "EawPatch"));
                f.WriteMeg("Data/Patch2.meg", m => m.Add("Init/EawConflict.bin", "EawPatch2"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("EawPatch2", ReadAll(gameRepo.OpenFile("Init/EawConflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_FocPatchOverridesEaw64Patch()
    {
        // All four EaW slots are loaded before any FoC slot, so even the latest EaW (64Patch)
        // gets overridden by the earliest FoC (Patch).
        //
        // The empty FoC Patch2/64Patch are necessary: the FoC ctor probes "Data/Patch2.meg" and
        // "Data/64Patch.meg" via the full lookup chain (mods → game → master → fallback). Without
        // empty game-dir shadows, the foc 64Patch probe would fall through to the fallback's 64Patch
        // and re-load the EaW entry as the very last write, defeating the test.
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.WriteMeg("Data/64Patch.meg",
                m => m.Add("Init/CrossConflict.bin", "Eaw64Patch")))
            .ConfigureGame(g =>
            {
                g.WriteMeg("Data/Patch.meg",   m => m.Add("Init/CrossConflict.bin", "FocPatch"));
                g.WriteMeg("Data/Patch2.meg",  _ => { });
                g.WriteMeg("Data/64Patch.meg", _ => { });
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("FocPatch", ReadAll(gameRepo.OpenFile("Init/CrossConflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_FocMegaFilesXmlOverridesEaw64Patch()
    {
        const string focMegaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/FocCustom.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.WriteMeg("Data/64Patch.meg",
                m => m.Add("Init/CrossConflict.bin", "Eaw64Patch")))
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", focMegaFilesXml);
                g.WriteMeg("Data/FocCustom.meg", m => m.Add("Init/CrossConflict.bin", "FocCustom"));
                g.WriteMeg("Data/Patch.meg",   _ => { });
                g.WriteMeg("Data/Patch2.meg",  _ => { });
                g.WriteMeg("Data/64Patch.meg", _ => { });  // shadow fallback's 64Patch
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("FocCustom", ReadAll(gameRepo.OpenFile("Init/CrossConflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void ErrorReporter_MissingEawPatches_AssertsFileNotFound_WhenFallbackConfigured()
    {
        using var repo = CreateBuilder()
            .WithFallbackGame(_ => { })
            .Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        var names = reporter.Asserts
            .Where(a => a.Kind == EngineAssertKind.FileNotFound)
            .Select(a => EngineFileName(a.Value))
            .ToList();
        // 6 missing: Patch / Patch2 / 64Patch in both the fallback and the FoC dir.
        Assert.Equal(2, names.Count(v => v == "Patch.meg"));
        Assert.Equal(2, names.Count(v => v == "Patch2.meg"));
        Assert.Equal(2, names.Count(v => v == "64Patch.meg"));
    }

    [Fact]
    public void ErrorReporter_NoFallbackConfigured_NoEawAttempts()
    {
        using var repo = CreateBuilder().Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        var fileNotFound = reporter.Asserts
            .Where(a => a.Kind == EngineAssertKind.FileNotFound)
            .ToList();
        Assert.Equal(3, fileNotFound.Count);
    }
}
