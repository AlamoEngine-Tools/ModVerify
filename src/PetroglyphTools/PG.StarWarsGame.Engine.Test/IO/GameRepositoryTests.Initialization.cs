using System.Linq;
using PG.StarWarsGame.Engine.ErrorReporting;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class GameRepositoryTests
{
    // ----------------------- pre-load: which MEGs are loaded at construction time -----------------------

    [Fact]
    public void Init_LoadsAllPatchMegs()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.WriteMeg("Data/Patch.meg",   m => m.Add("Init/InPatch.bin",   "Patch"));
                g.WriteMeg("Data/Patch2.meg",  m => m.Add("Init/InPatch2.bin",  "Patch2"));
                g.WriteMeg("Data/64Patch.meg", m => m.Add("Init/In64Patch.bin", "64Patch"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("Patch",   ReadAll(gameRepo.OpenFile("Init/InPatch.bin",   megFileOnly: true)));
        Assert.Equal("Patch2",  ReadAll(gameRepo.OpenFile("Init/InPatch2.bin",  megFileOnly: true)));
        Assert.Equal("64Patch", ReadAll(gameRepo.OpenFile("Init/In64Patch.bin", megFileOnly: true)));
    }

    [Fact]
    public void Init_LoadsMegsListedInMegaFilesXml()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/First.meg</File>
              <File>Data/Second.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/First.meg",  m => m.Add("Init/InFirst.bin",  "First"));
                g.WriteMeg("Data/Second.meg", m => m.Add("Init/InSecond.bin", "Second"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("First",  ReadAll(gameRepo.OpenFile("Init/InFirst.bin",  megFileOnly: true)));
        Assert.Equal("Second", ReadAll(gameRepo.OpenFile("Init/InSecond.bin", megFileOnly: true)));
    }

    // ----------------------- master MEG ordering: later load wins -----------------------

    [Fact]
    public void MasterMeg_Patch2OverridesPatch()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.WriteMeg("Data/Patch.meg",  m => m.Add("Init/Conflict.bin", "Patch"));
                g.WriteMeg("Data/Patch2.meg", m => m.Add("Init/Conflict.bin", "Patch2"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("Patch2", ReadAll(gameRepo.OpenFile("Init/Conflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_64PatchOverridesPatch2()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.WriteMeg("Data/Patch2.meg",  m => m.Add("Init/Conflict.bin", "Patch2"));
                g.WriteMeg("Data/64Patch.meg", m => m.Add("Init/Conflict.bin", "64Patch"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("64Patch", ReadAll(gameRepo.OpenFile("Init/Conflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_PatchOverridesMegaFilesXmlEntries()
    {
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
                g.WriteMeg("Data/Custom.meg", m => m.Add("Init/Conflict.bin", "Custom"));
                g.WriteMeg("Data/Patch.meg",  m => m.Add("Init/Conflict.bin", "Patch"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("Patch", ReadAll(gameRepo.OpenFile("Init/Conflict.bin", megFileOnly: true)));
    }

    [Fact]
    public void MasterMeg_MegaFilesXml_LaterMegOverridesEarlier()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/First.meg</File>
              <File>Data/Second.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/First.meg",  m => m.Add("Init/Conflict.bin", "First"));
                g.WriteMeg("Data/Second.meg", m => m.Add("Init/Conflict.bin", "Second"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("Second", ReadAll(gameRepo.OpenFile("Init/Conflict.bin", megFileOnly: true)));
    }

    // ----------------------- error reporter signals at init -----------------------

    [Fact]
    public void ErrorReporter_MissingPatchMegs_AssertsFileNotFound()
    {
        // Empty repo (no fallback): the ctor will probe each patch slot and miss all three.
        using var repo = CreateBuilder().Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        var fileNotFoundNames = reporter.Asserts
            .Where(a => a.Kind == EngineAssertKind.FileNotFound)
            .Select(a => EngineFileName(a.Value))
            .ToList();
        Assert.Contains("Patch.meg",   fileNotFoundNames);
        Assert.Contains("Patch2.meg",  fileNotFoundNames);
        Assert.Contains("64Patch.meg", fileNotFoundNames);
    }

    [Fact]
    public void ErrorReporter_AllPatchesPresent_NoFileNotFoundForPatches()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.WriteMeg("Data/Patch.meg",   _ => { });
                g.WriteMeg("Data/Patch2.meg",  _ => { });
                g.WriteMeg("Data/64Patch.meg", _ => { });
            })
            .Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        var fileNotFoundNames = reporter.Asserts
            .Where(a => a.Kind == EngineAssertKind.FileNotFound)
            .Select(a => EngineFileName(a.Value))
            .ToList();
        Assert.DoesNotContain("Patch.meg",   fileNotFoundNames);
        Assert.DoesNotContain("Patch2.meg",  fileNotFoundNames);
        Assert.DoesNotContain("64Patch.meg", fileNotFoundNames);
    }

    [Fact]
    public void ErrorReporter_MegaFilesXmlReferencesMissingMeg_AssertsFileNotFound()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/DoesNotExist.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/MegaFiles.xml", megaFilesXml))
            .Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        Assert.Contains(reporter.Asserts, a =>
            a.Kind == EngineAssertKind.FileNotFound && EngineFileName(a.Value) == "DoesNotExist.meg");
    }

    [Fact]
    public void ErrorReporter_MissingSpeechMeg_DoesNotAssert()
    {
        // Speech.meg paths are intentionally silent: missing speech files only emit a debug log.
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/EnglishSpeech.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/MegaFiles.xml", megaFilesXml))
            .Build();
        var reporter = new RecordingErrorReporter();

        _ = CreateRepository(repo, reporter);

        Assert.DoesNotContain(reporter.Asserts, a =>
            a.Kind == EngineAssertKind.FileNotFound && EngineFileName(a.Value) == "EnglishSpeech.meg");
    }
}
