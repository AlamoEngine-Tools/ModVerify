using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerifyTool;

internal class GameNotFoundException(string message) : GameException(message);