using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerifyTool.GameFinder;

internal record GameFinderResult(IGame Game, IGame? FallbackGame);