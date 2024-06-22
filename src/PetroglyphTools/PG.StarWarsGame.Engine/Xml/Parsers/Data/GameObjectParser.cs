using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class GameObjectParser(IServiceProvider serviceProvider) : XmlObjectParser<GameObject>(serviceProvider)
{
    private readonly ICrc32HashingService _crc32Hashing = serviceProvider.GetRequiredService<ICrc32HashingService>();

    protected override bool IsTagSupported(string tag)
    {
        throw new NotImplementedException();
    }
    
    public override GameObject Parse(XElement element)
    {
        throw new NotSupportedException();
    }

    public override GameObject Parse(XElement element, IReadOnlyValueListDictionary<Crc32, GameObject> parsedElements, out Crc32 nameCrc)
    {
        var properties = ToKeyValuePairList(element);
        var name = GetNameAttributeValue(element);
        nameCrc = _crc32Hashing.GetCrc32Upper(name.AsSpan(), PGConstants.PGCrc32Encoding);
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

    public string GetTagName(XElement element)
    {
        return element.Name.LocalName;
    }
}