namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Engine-agnostic tests for the base <see cref="PG.StarWarsGame.Engine.IO.IGameRepository"/> file lookup.
/// </summary>
public abstract partial class GameRepositoryTests : EngineRepositoryTestBase
{
    protected override CaseInsensitivityFixture BuildCaseInsensitivityFixture()
    {
        return new CaseInsensitivityFixture(
            PopulateGame: g =>
            {
                g.Write("Data/XML/Foo.xml", "fs-content");
                g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Audio/Bar.wav", "meg-content"));
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
}
