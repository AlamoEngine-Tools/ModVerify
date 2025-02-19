using System;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public static class GameObjectXmlTags
{
    public const string LandTerrainModelMapping =  "Land_Terrain_Model_Mapping";
    public const string GalacticModelName = "Galactic_Model_Name";
    public const string DestroyedGalacticModelName = "Destroyed_Galactic_Model_Name";
    public const string LandModelName = "Land_Model_Name";
    public const string SpaceModelName = "Space_Model_Name";
    public const string ModelName = "Model_Name";
    public const string TacticalModelName = "Tactical_Model_Name";
    public const string GalacticFleetOverrideModelName = "Galactic_Fleet_Override_Model_Name";
    public const string GuiModelName = "GUI_Model_Name";
    public const string LandModelAnimOverrideName = "Land_Model_Anim_Override_Name";
    public const string XxxSpaceModelName = "xxxSpace_Model_Name";
    public const string DamagedSmokeAssetName = "Damaged_Smoke_Asset_Name";
}

public sealed class GameObjectParser(
    IReadOnlyValueListDictionary<Crc32, GameObject> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParser<GameObject>(parsedElements, serviceProvider, errorReporter)
{ 
    public override GameObject Parse(XElement element, out Crc32 crc32)
    {
        var name = GetXmlObjectName(element, out crc32, true);
        var type = GetTagName(element);
        var objectType = EstimateType(type);
        var gameObject = new GameObject(type, name, crc32, objectType, XmlLocationInfo.FromElement(element));

        Parse(gameObject, element, default);

        return gameObject;
    }

    protected override bool ParseTag(XElement tag, GameObject xmlObject)
    {
        switch (tag.Name.LocalName)
        {
            case GameObjectXmlTags.LandTerrainModelMapping:
                var mappingValue = CommaSeparatedStringKeyValueListParser.Instance.Parse(tag);
                var dict = xmlObject.InternalLandTerrainModelMapping;
                foreach (var keyValuePair in mappingValue)
                {
                    if (!dict.ContainsKey(keyValuePair.key))
                        dict.Add(keyValuePair.key, keyValuePair.value);
                }
                return true;
            case GameObjectXmlTags.GalacticModelName:
                xmlObject.GalacticModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.DestroyedGalacticModelName:
                xmlObject.DestroyedGalacticModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.LandModelName:
                xmlObject.LandModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.SpaceModelName:
                xmlObject.SpaceModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.ModelName:
                xmlObject.ModelName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.TacticalModelName:
                xmlObject.TacticalModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.GalacticFleetOverrideModelName:
                xmlObject.GalacticFleetOverrideModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.GuiModelName:
                xmlObject.GuiModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.LandModelAnimOverrideName:
                xmlObject.LandAnimOverrideModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.XxxSpaceModelName:
                xmlObject.XxxSpaceModeModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case GameObjectXmlTags.DamagedSmokeAssetName:
                xmlObject.DamagedSmokeAssetModel = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            default: return true; // TODO: Once parsing is complete, switch to false.
        }
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