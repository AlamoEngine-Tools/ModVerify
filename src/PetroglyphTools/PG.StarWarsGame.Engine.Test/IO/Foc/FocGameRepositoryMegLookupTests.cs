using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO.Foc;

public class FocGameRepositoryMegLookupTests : GameRepositoryMegLookupTests
{
    protected override GameEngineType Engine => GameEngineType.Foc;

    [Fact]
    public void FocPatchOverridesEawFallbackPatch()
    {
        // FoC-specific: FoC loads MEGs from both the FoC directory and the EaW fallback directory,
        // with FoC entries overriding EaW entries via replaceExisting in the virtual MEG archive.
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/conflict.wav", "eaw")))
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/conflict.wav", "foc")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("foc", ReadAll(gameRepo.OpenFile("Data/Audio/conflict.wav", megFileOnly: true)));
    }

    [Fact]
    public void EawFallbackPatch_FoundWhenNotShadowed()
    {
        // FoC-specific: the EaW fallback's Patch.meg is loaded transparently when FoC doesn't shadow it.
        using var repo = CreateBuilder()
            .WithFallbackGame(f => f.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Audio/eaw-only.wav", "eaw")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("eaw", ReadAll(gameRepo.OpenFile("Data/Audio/eaw-only.wav", megFileOnly: true)));
    }
}
