using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace PG.StarWarsGame.Engine.Database;

public class GameInitializationOptions
{
    public required GameEngineType TargetEngineType { get; init; }

    public required GameLocations Locations { get; init; }

    public bool CancelOnError { get; init; }

    public IDatabaseErrorListener? ErrorListener { get; init; }
}