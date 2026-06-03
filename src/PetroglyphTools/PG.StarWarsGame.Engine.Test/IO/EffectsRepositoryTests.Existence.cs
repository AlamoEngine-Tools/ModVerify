using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract partial class EffectsRepositoryTests : EngineRepositoryTestBase
{
    protected override CaseInsensitivityFixture BuildCaseInsensitivityFixture()
    {
        return new CaseInsensitivityFixture(
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
    }

    protected override RepositoryPriorityFixture BuildPriorityFixture()
    {
        return new RepositoryPriorityFixture(
            SelectRepository: gameRepo => gameRepo.EffectsRepository,
            ResolvablePath: "Data/Art/Shaders/MyShader.fx");
    }

    public static TheoryData<string, string> ResolvableShaderLocations_Root()
    {
        return ShaderLocationsTheoryData(
            "MyShader.fx",
            "MyShader.fxo",
            "MyShader.fxh");
    }

    public static TheoryData<string, string> ResolvableShaderLocations_DataArts()
    {
        return ShaderLocationsTheoryData(
            "Data/Art/Shaders/MyShader.fx",
            "Data/Art/Shaders/Terrain/MyShader.fx");
    }

    [Theory]
    [MemberData(nameof(ResolvableShaderLocations_Root))]
    [MemberData(nameof(ResolvableShaderLocations_DataArts))]
    public void FileExists_ResolvesShaderAtSupportedLocation(string writtenPath, string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(writtenPath, "x"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [InlineData("Engine\\MyShader")]
    [InlineData("Engine/MyShader")]
    [InlineData("Engine\\MyShader.fx")]
    [InlineData("Engine\\MyShader.bogus")]
    public void FileExists_ShaderAddressedWithEngineSubdirPrefix_Resolves(string input)
    {
        // The Engine directory is not one of the hardcoded shader search paths, but a shader placed there is
        // still reachable when the request carries the "Engine\" prefix: it resolves under the SHADERS base.
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write("Data/Art/Shaders/Engine/MyShader.fx", "engine-fx"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
        Assert.Equal("engine-fx", ReadAll(gameRepo.EffectsRepository.OpenFile(input)));
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
            .ConfigureGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("MyShader.fx", "fx-in-meg")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    #region Mods

    [Theory]
    [MemberData(nameof(ResolvableShaderLocations_Root))]
    public void FileExists_ShaderInModRoot_ShouldNotResolve(string writtenPath, string input)
    { 
        using var repo = CreateBuilder()
            .ConfigureGame(g => { })
            .WithMod("MyMod", w => w.Write(writtenPath, "fx-in-mod"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ResolvableShaderLocations_DataArts))]
    public void FileExists_ShaderInModDataArts_Resolves(string writtenPath, string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => { })
            .WithMod("MyMod", w => w.Write(writtenPath, "fx-in-mod"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    #endregion

    #region Fallback

    [Theory]
    [MemberData(nameof(ResolvableShaderLocations_Root))]
    public void FileExists_ShaderInFallbackRoot_ShouldNotResolve(string writtenPath, string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => { })
            .WithFallback("fallback",w => w.Write(writtenPath, "fx-in-fallback"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.EffectsRepository.FileExists(input));
    }

    [Theory]
    [MemberData(nameof(ResolvableShaderLocations_DataArts))]
    public void FileExists_ShaderInFallbackDataArts_Resolves(string writtenPath, string input)
    {
        using var repo = CreateBuilder()
            .ConfigureGame(g => { })
            .WithFallback("fallback", w => w.Write(writtenPath, "fx-in-fallback"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.EffectsRepository.FileExists(input));
    }

    #endregion

    private static TheoryData<string, string> ShaderLocationsTheoryData(params string[] locations)
    {
        var data = new TheoryData<string, string>();
        foreach (var location in locations)
        foreach (var input in ShaderTestData.EquivalentShaderNames)
            data.Add(location, input);
        return data;
    }
}
