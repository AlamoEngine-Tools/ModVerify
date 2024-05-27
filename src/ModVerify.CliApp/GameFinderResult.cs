using PG.StarWarsGame.Infrastructure.Games;

namespace ModVerify.CliApp;

public readonly record struct GameFinderResult(IGame Game, IGame FallbackGame);