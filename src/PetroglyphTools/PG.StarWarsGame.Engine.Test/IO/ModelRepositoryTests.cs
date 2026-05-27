using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class ModelRepositoryTests : EngineRepositoryTestBase
{
    protected override RepositoryLookupSetup GetLookupSetup() => new(
        PopulateGame: g =>
        {
            g.Write("Data/Art/Models/Ship.alo", "fs-alo");
            g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Art/Models/OtherShip.alo", "meg-alo"));
        },
        SelectRepository: gameRepo => gameRepo.ModelRepository,
        FilesystemLookup: "Data/Art/Models/Ship.alo",
        FilesystemContent: "fs-alo",
        MegLookup: "Data/Art/Models/OtherShip.alo",
        MegContent: "meg-alo");

    [Fact]
    public void FileExists_AsIs_ReturnsTrue()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Models/Ship.alo", "alo"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.ModelRepository.FileExists("Data/Art/Models/Ship.alo"));
    }

    [Fact]
    public void FileExists_DifferentExtension_FallsBackToAlo()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.Write("Data/Art/Models/Ship.ALO", "alo"))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.ModelRepository.FileExists("Data/Art/Models/Ship.fbx"));
    }

    [Fact]
    public void FileExists_MissingModel_ReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.ModelRepository.FileExists("Data/Art/Models/DoesNotExist.alo"));
    }

    [Fact]
    public void FileExists_EmptyPath_ReturnsFalse()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.False(gameRepo.ModelRepository.FileExists(""));
    }

    [Fact]
    public void FileExists_ModelInMeg_Resolves()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Art/Models/Ship.alo", "alo")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.ModelRepository.FileExists("Data/Art/Models/Ship.alo"));
    }

    [Fact]
    public void FileExists_DifferentExtensionInMeg_FallsBackToAlo()
    {
        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg("Data/Patch.meg",
                meg => meg.Add("Data/Art/Models/Ship.alo", "alo")))
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.True(gameRepo.ModelRepository.FileExists("Data/Art/Models/Ship.fbx"));
    }
}
