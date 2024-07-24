using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace ModVerify.CliApp;

internal class VerifyGameSetupData
{
    public required IPhysicalPlayableObject VerifyObject { get; init; }

    public required GameEngineType EngineType { get; init; }

    public required GameLocations GameLocations { get; init; }
}