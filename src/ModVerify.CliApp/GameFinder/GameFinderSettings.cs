using PG.StarWarsGame.Engine;

namespace AET.ModVerify.App.GameFinder;

internal sealed class GameFinderSettings
{
    internal static readonly GameFinderSettings Default = new();
    
    public bool InitMods { get; init; } = true;
    
    public bool SearchFallbackGame { get; init; } = true;

    public GameEngineType? Engine { get; init; } = null;
}