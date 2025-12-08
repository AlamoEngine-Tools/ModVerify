using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerify.App.GameFinder;

internal record GameFinderResult(IGame Game, IGame? FallbackGame);