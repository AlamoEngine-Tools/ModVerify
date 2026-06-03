using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class TextureRepositoryTests : ExtensionFallbackRepositoryTests
{
    protected override bool ResolvesFileNameWithoutDirectory => true;

    protected override bool SurfacesPathTooLong => true;

    protected override string FallbackExtension => ".dds";

    protected override string SecondaryExtension => ".tga";

    protected override CaseInsensitivityFixture BuildCaseInsensitivityFixture()
    {
        return new CaseInsensitivityFixture(
            PopulateGame: g =>
            {
                g.Write("Data/Art/Textures/MyTex.tga", "fs-tga");
                g.RegisterAndWriteMeg("Data/Textures.meg", meg => meg.Add("Data/Art/Textures/OtherTex.tga", "meg-tga"));
            },
            SelectRepository: gameRepo => gameRepo.TextureRepository,
            FilesystemLookup: "Data/Art/Textures/MyTex.tga",
            FilesystemContent: "fs-tga",
            MegLookup: "Data/Art/Textures/OtherTex.tga",
            MegContent: "meg-tga");
    }

    protected override RepositoryFixture BuildRepositoryFixture()
    {
        return new RepositoryFixture(
            SelectRepository: gameRepo => gameRepo.TextureRepository,
            ResolvablePath: "Data/Art/Textures/MyTex.tga");
    }

    [Fact]
    public void Priority_AsIsLocationBeatsTexturesDirectory()
    {
        // For a bare request the path is probed as-is (here: the game root) before it is retried under the
        // implicit ./Data/Art/Textures/ directory.
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write("MyTex.tga", "root");
                g.Write("Data/Art/Textures/MyTex.tga", "textures-dir");
            })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal("root", ReadAll(gameRepo.TextureRepository.OpenFile("MyTex.tga")));
    }
}
