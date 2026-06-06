using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class GameRepositoryTests
{
    [Fact]
    public void FileExists_OutParams_HitInMeg_SetsInMegAndNormalizedPath()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
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
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", payload)))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(payload, ReadAll(gameRepo.OpenFile("Data/Audio/foo.wav")));
    }

    [Fact]
    public void FileExists_MegLookup_CaseInsensitive()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
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
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.FileExists("Data/Audio/foo.wav"));
        Assert.True(gameRepo.FileExists(@"Data\Audio\foo.wav"));
    }

    [Fact]
    public void FileExists_FileSystemWinsOverMeg()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
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
            .ConfigureGame(g =>
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
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/foo.wav", "x")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("Data/Audio/missing.wav"));
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
            .ConfigureGame(g => g.Write("Data/MegaFiles.xml", megaFilesXml))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.FileExists("anything.txt"));
    }

    [Fact]
    public void RegisterAndWriteMeg_LoadsMegViaGeneratedMegaFilesXml()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.RegisterAndWriteMeg("Data/Custom.meg",
                m => m.Add(TextEntry, "registered")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("registered", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }

    [Fact]
    public void RegisterAndWriteMeg_RegistrationOrderIsLoadOrder()
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.RegisterAndWriteMeg("Data/First.meg", m => m.Add(TextEntry, "first"));
                g.RegisterAndWriteMeg("Data/Second.meg", m => m.Add(TextEntry, "second"));
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("second", ReadAll(gameRepo.OpenFile(TextEntry, megFileOnly: true)));
    }
}
