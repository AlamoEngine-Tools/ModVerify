using PG.StarWarsGame.Infrastructure.Games;

namespace ModVerify.CliApp;

internal record GameFinderResult(IGame Game, IGame? FallbackGame);