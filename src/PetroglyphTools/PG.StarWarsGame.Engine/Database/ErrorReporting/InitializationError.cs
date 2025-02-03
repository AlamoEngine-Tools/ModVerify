namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public sealed class InitializationError
{
    public required string GameManager { get; init; }

    public required string Message { get; init; }
}