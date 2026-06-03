namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class ModelRepositoryTests : ExtensionFallbackRepositoryTests
{
    protected override bool ResolvesFileNameWithoutDirectory => false;

    protected override bool SurfacesPathTooLong => true;

    protected override string FallbackExtension => ".alo";

    protected override string SecondaryExtension => ".ala";

    protected override CaseInsensitivityFixture BuildCaseInsensitivityFixture()
    {
        return new CaseInsensitivityFixture(
            PopulateGame: g =>
            {
                g.Write("Data/Art/Models/Ship.alo", "fs-alo");
                g.RegisterAndWriteMeg("Data/Models.meg", meg => meg.Add("Data/Art/Models/OtherShip.alo", "meg-alo"));
            },
            SelectRepository: gameRepo => gameRepo.ModelRepository,
            FilesystemLookup: "Data/Art/Models/Ship.alo",
            FilesystemContent: "fs-alo",
            MegLookup: "Data/Art/Models/OtherShip.alo",
            MegContent: "meg-alo");
    }

    protected override RepositoryFixture BuildRepositoryFixture()
    {
        return new RepositoryFixture(
            SelectRepository: gameRepo => gameRepo.ModelRepository,
            ResolvablePath: "Data/Art/Models/Ship.alo");
    }
}
