using System.Collections.Generic;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine.GameObjects;

// TODO: Not sure, this is the correct namespace

public enum MapEnvironmentType
{
    Temperate = 0x0,
    Arctic = 0x1,
    Desert = 0x2,
    Forest = 0x3,
    Swamp = 0x4,
    Volcanic = 0x5,
    Urban = 0x6,
    Space = 0x7,
}

// TODO: To separate GameManager that holds these Conversion instances
public static class MapEnvironmentTypeConversion
{
    public static readonly EnumConversionDictionary<MapEnvironmentType> Dictionary = new([
        new KeyValuePair<string, MapEnvironmentType>("Temperate", MapEnvironmentType.Temperate),
        new KeyValuePair<string, MapEnvironmentType>("Arctic", MapEnvironmentType.Arctic),
        new KeyValuePair<string, MapEnvironmentType>("Desert", MapEnvironmentType.Desert),
        new KeyValuePair<string, MapEnvironmentType>("Forest", MapEnvironmentType.Forest),
        new KeyValuePair<string, MapEnvironmentType>("Swamp", MapEnvironmentType.Swamp),
        new KeyValuePair<string, MapEnvironmentType>("Volcanic", MapEnvironmentType.Volcanic),
        new KeyValuePair<string, MapEnvironmentType>("Urban", MapEnvironmentType.Urban),
        new KeyValuePair<string, MapEnvironmentType>("Space", MapEnvironmentType.Space)
    ]);
}