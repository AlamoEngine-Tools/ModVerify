using PG.StarWarsGame.Infrastructure.Games;

namespace ModVerify.CliApp;

internal class GameNotFoundException(string message) : GameException(message);