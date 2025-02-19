using PG.StarWarsGame.Engine.ErrorReporting;

namespace PG.StarWarsGame.Engine.Database;

public class GameInitializationOptions
{
    public required GameEngineType TargetEngineType { get; init; }

    public required GameLocations Locations { get; init; }

    public bool CancelOnError { get; init; }

    public IGameErrorReporter? GameErrorReporter { get; init; }
}