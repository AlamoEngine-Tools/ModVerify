using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerify.App.GameFinder;

internal class GameNotFoundException(string message) : GameException(message);