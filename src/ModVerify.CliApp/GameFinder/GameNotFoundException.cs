using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerifyTool.GameFinder;

internal class GameNotFoundException(string message) : GameException(message);