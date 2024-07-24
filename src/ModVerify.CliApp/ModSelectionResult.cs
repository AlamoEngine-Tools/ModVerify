using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;

namespace ModVerify.CliApp;

internal record ModSelectionResult
{
    public IPhysicalPlayableObject ModOrGame { get; init; }
    public IGame? FallbackGame { get; init; }
}