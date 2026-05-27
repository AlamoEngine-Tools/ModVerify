using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class GameRepositoryMegLookupTests : EngineRepositoryTestBase
{
    [Fact]
    public void FileExists_InPatchMeg_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "audio-bytes")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Audio/foo.wav"));
    }

    [Fact]
    public void FileExists_OutParams_HitInMeg_SetsInMegAndNormalizedPath()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "audio")))
            .Build();
        var gameRepo = CreateRepository(repo);

        var found = gameRepo.FileExists("Data/Audio/foo.wav", megFileOnly: false, out var inMeg, out var actualPath);

        Assert.True(found);
        Assert.True(inMeg);
        Assert.Equal(@"DATA\AUDIO\FOO.WAV", actualPath);
    }

    [Fact]
    public void OpenFile_FromPatchMeg_ReturnsEntryBytes()
    {
        const string payload = "wav-payload";
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", payload)))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(payload, ReadAll(gameRepo.OpenFile("Data/Audio/foo.wav")));
    }

    [Fact]
    public void FileExists_InPatch2Meg_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch2.meg",
                meg => meg.Add("Data/Audio/two.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Audio/two.wav"));
    }

    [Fact]
    public void FileExists_In64PatchMeg_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/64Patch.meg",
                meg => meg.Add("Data/Audio/sixtyfour.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Audio/sixtyfour.wav"));
    }

    [Fact]
    public void FileExists_MegListedInMegaFilesXml_ReturnsTrue()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
              <File>Data/Custom.meg</File>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("Data/MegaFiles.xml", megaFilesXml);
                g.WriteMeg("Data/Custom.meg", meg => meg.Add("Data/Custom/Entry.txt", "x"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Custom/Entry.txt"));
    }

    [Fact]
    public void FileExists_MegLookup_CaseInsensitive()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("DATA/AUDIO/FOO.WAV"));
        Assert.True(gameRepo.FileExists("data/audio/foo.wav"));
        Assert.True(gameRepo.FileExists("Data/Audio/Foo.Wav"));
    }

    [Fact]
    public void FileExists_MegLookup_SeparatorInsensitive()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Audio/foo.wav"));
        Assert.True(gameRepo.FileExists(@"Data\Audio\foo.wav"));
    }

    [Fact]
    public void FileExists_FilesystemWinsOverMeg()
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("Data/Audio/foo.wav", "from-fs");
                g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Audio/foo.wav", "from-meg"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        var found = gameRepo.FileExists("Data/Audio/foo.wav", megFileOnly: false, out var inMeg, out _);
        Assert.True(found);
        Assert.False(inMeg);

        Assert.Equal("from-fs", ReadAll(gameRepo.OpenFile("Data/Audio/foo.wav")));
    }

    [Fact]
    public void FileExists_MegFileOnlyFlag_SkipsFilesystemEvenIfPresent()
    {
        using var repo = CreateBuilder()
            .WithGame(g =>
            {
                g.Write("Data/Audio/foo.wav", "from-fs");
                g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Audio/foo.wav", "from-meg"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        var found = gameRepo.FileExists("Data/Audio/foo.wav", megFileOnly: true, out var inMeg, out _);
        Assert.True(found);
        Assert.True(inMeg);

        Assert.Equal("from-meg", ReadAll(gameRepo.OpenFile("Data/Audio/foo.wav", megFileOnly: true)));
    }

    [Fact]
    public void FileExists_MissingMegEntry_ReturnsFalse()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Data/Audio/missing.wav"));
    }

    [Fact]
    public void EmptyRepository_NoErrors_LookupReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("anything.txt"));
        Assert.Null(gameRepo.TryOpenFile("anything.txt"));
    }

    [Fact]
    public void MegLookup_OverlongPath_ReturnsFalse()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        var path = new string('a', 300) + ".wav";
        Assert.False(gameRepo.FileExists(path));
    }

    [Fact]
    public void EmptyMegaFilesXml_DoesNotCrash()
    {
        const string megaFilesXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <MegaFiles>
            </MegaFiles>
            """;
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/MegaFiles.xml", megaFilesXml))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("anything.txt"));
    }
}
