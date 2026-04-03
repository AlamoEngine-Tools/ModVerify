namespace PG.StarWarsGame.Files.TED.Binary;

internal enum TedChunkType : uint
{
    MapInfo = 0x0,
    StartPositions = 0x3,
    MapPreviewImageData = 0x13,
}

internal enum MapInfoChunkType : uint
{
    Version = 0,
    Type = 1,
    PlayerCount = 2,
    MapLevels = 3,
    SupportedWeather = 4,
    FactionOwner = 5,
    MapType = 6,
    SupportedSystems = 7,
    MapName = 8,
    PlanetName = 9,
    GameTypes = 10,
    CustomMap = 11,
    Width = 16,
    Height = 17,
    NewMultiplayerMarkerSystem = 18,
}