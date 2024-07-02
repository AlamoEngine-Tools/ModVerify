using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class GameObjectParser(
    IReadOnlyValueListDictionary<Crc32, GameObject> parsedElements,
    IServiceProvider serviceProvider)
    : XmlObjectParser<GameObject>(parsedElements, serviceProvider)
{
    protected override IPetroglyphXmlElementParser? GetParser(string tag)
    {
        switch (tag)
        {
            case "Land_Terrain_Model_Mapping":
                return PrimitiveParserProvider.CommaSeparatedStringKeyValueListParser;
            case "Galactic_Model_Name":
            case "Destroyed_Galactic_Model_Name":
            case "Land_Model_Name":
            case "Space_Model_Name":
            case "Model_Name":
            case "Tactical_Model_Name":
            case "Galactic_Fleet_Override_Model_Name":
            case "GUI_Model_Name":
            case "GUI_Model":
            case "Land_Model_Anim_Override_Name":
            case "xxxSpace_Model_Name":
            case "Damaged_Smoke_Asset_Name":
                return PrimitiveParserProvider.StringParser;
            default:
                return null;
        }
    }

    public override GameObject Parse(XElement element, out Crc32 nameCrc)
    {
        var properties = ParseXmlElement(element);
        var name = GetNameAttributeValue(element);
        nameCrc = HashingService.GetCrc32Upper(name.AsSpan(), PGConstants.PGCrc32Encoding);
        var type = GetTagName(element);
        var objectType = EstimateType(type);
        var gameObject = new GameObject(type, name, nameCrc, objectType, properties, XmlLocationInfo.FromElement(element));
        return gameObject;
    }

    private static GameObjectType EstimateType(string tagName)
    {
        if (tagName.StartsWith("Props_"))
            return GameObjectType.Prop;
        if (tagName.StartsWith("CIN_", StringComparison.OrdinalIgnoreCase))
            return GameObjectType.CinematicObject;

        return tagName switch
        {
            "Container" => GameObjectType.Container,
            "GenericHeroUnit" => GameObjectType.GenericHeroUnit,
            "GroundBase" => GameObjectType.GroundBase,
            "GroundBuildable" => GameObjectType.GroundBuildable,
            "GroundCompany" => GameObjectType.GroundCompany,
            "GroundInfantry" => GameObjectType.GroundInfantry,
            "GroundStructure" => GameObjectType.GroundStructure,
            "GroundVehicle" => GameObjectType.GroundVehicle,
            "HeroCompany" => GameObjectType.HeroCompany,
            "HeroUnit" => GameObjectType.HeroUnit,
            "Indigenous_Unit" => GameObjectType.IndigenousUnit,
            "LandBombingUnit" => GameObjectType.LandBombingUnit,
            "LandPrimarySkydome" => GameObjectType.LandPrimarySkydome,
            "LandSecondarySkydome" => GameObjectType.LandSecondarySkydome,
            "Marker" => GameObjectType.Marker,
            "MiscObject" => GameObjectType.MiscObject,
            "Mobile_Defense_Unit" => GameObjectType.MobileDefenseUnit,
            "MultiplayerStructureMarker" => GameObjectType.MultiplayerStructureMarker,
            "Particle" => GameObjectType.Particle,
            "Planet" => GameObjectType.Planet,
            "Projectile" => GameObjectType.Projectile,
            "ScriptMarker" => GameObjectType.ScriptMarker,
            "SecondaryStructure" => GameObjectType.SecondaryStructure,
            "SlaveCompany" => GameObjectType.SlaveCompany,
            "Slave_Unit" => GameObjectType.SlaveUnit,
            "SpaceBuildable" => GameObjectType.SpaceBuildable,
            "SpacePrimarySkydome" => GameObjectType.SpacePrimarySkydome,
            "SpaceProp" => GameObjectType.SpaceProp,
            "SpaceSecondarySkydome" => GameObjectType.SpaceSecondarySkydome,
            "SpaceUnit" => GameObjectType.SpaceUnit,
            "SpecialEffect" => GameObjectType.SpecialEffect,
            "SpecialStructure" => GameObjectType.SpecialStructure,
            "Squadron" => GameObjectType.Squadron,
            "StarBase" => GameObjectType.StarBase,
            "TechBuilding" => GameObjectType.TechBuilding,
            "TransportUnit" => GameObjectType.TransportUnit,
            "UniqueUnit" => GameObjectType.UniqueUint,
            "UpgradeObject" => GameObjectType.UpgradeUnit,
            _ => GameObjectType.Unknown
        };
    }

    public override GameObject Parse(XElement element) => throw new NotSupportedException();
}