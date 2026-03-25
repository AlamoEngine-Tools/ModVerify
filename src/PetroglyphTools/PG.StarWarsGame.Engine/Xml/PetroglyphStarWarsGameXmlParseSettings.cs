namespace PG.StarWarsGame.Engine.Xml;

public sealed record PetroglyphStarWarsGameXmlParseSettings
{
    public required string GameManager { get; init; }

    public bool InvalidFilesListXmlFailsInitialization { get; init; } = true;

    public bool InvalidObjectXmlFailsInitialization { get; init; } = false;
}