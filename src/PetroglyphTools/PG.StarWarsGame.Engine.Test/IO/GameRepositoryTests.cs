using PG.StarWarsGame.Engine.IO;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Engine-agnostic tests for the base <see cref="IGameRepository"/> file lookup.
/// </summary>
public abstract partial class GameRepositoryTests : EngineRepositoryTestBase
{
    protected override CaseInsensitivityFixture BuildCaseInsensitivityFixture()
    {
        return new CaseInsensitivityFixture(
            PopulateGame: g =>
            {
                g.Write("Data/XML/Foo.xml", "fs-content");
                g.RegisterAndWriteMeg("Data/Content.meg", meg => meg.Add("Data/Audio/Bar.wav", "meg-content"));
            },
            SelectRepository: gameRepo => gameRepo,
            FilesystemLookup: "Data/XML/Foo.xml",
            FilesystemContent: "fs-content",
            MegLookup: "Data/Audio/Bar.wav",
            MegContent: "meg-content");
    }

    protected override RepositoryPriorityFixture BuildPriorityFixture()
    {
        return new RepositoryPriorityFixture(
            SelectRepository: gameRepo => gameRepo,
            ResolvablePath: "Data/XML/Foo.xml");
    }

    [Fact]
    public void Path_NoModConfigured_PointsToGameDirectory()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(RootPathOf(gameRepo, repo.GameLocations.GamePath), gameRepo.Path);
    }

    [Fact]
    public void Path_ModsConfigured_PointsToFirstMod()
    {
        using var repo = CreateBuilder()
            .WithMod("FirstMod", _ => { })
            .WithMod("SecondMod", _ => { })
            .Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(RootPathOf(gameRepo, repo.GameLocations.ModPaths[0]), gameRepo.Path);
    }

    // The repository's Path is the fully-qualified top-most root (first mod, else game directory) with a
    // trailing directory separator, resolved through the same file system the repository uses.
    private static string RootPathOf(IGameRepository gameRepo, string rawRoot)
    {
        var path = gameRepo.PGFileSystem.UnderlyingFileSystem.Path;
        return path.GetFullPath(rawRoot) + path.DirectorySeparatorChar;
    }
}
