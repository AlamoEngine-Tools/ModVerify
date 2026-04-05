using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.Xml.Parsers.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using Crc32 = PG.Commons.Hashing.Crc32;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal partial class GameObjectTypeParser(
    GameEngineType engine,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : NamedXmlObjectParser<GameObjectType>(engine, new GameObjectXmlTagMapper(serviceProvider), errorReporter, serviceProvider)
{
    internal bool OverlayLoad { get; set; }

    protected override bool UpperCaseNameForCrc => true;
    
    protected override bool UpperCaseNameForObject => true;

    protected override GameObjectType CreateXmlObject(
        string name, 
        Crc32 nameCrc, 
        XElement element, 
        IReadOnlyFrugalValueListDictionary<Crc32, GameObjectType> parsedEntries, 
        XmlLocationInfo location)
    {
        if (OverlayLoad)
        {
            parsedEntries.TryGetFirstValue(nameCrc, out var type);
            Debug.Assert(type is not null);
            OverlayType(type, element, parsedEntries);
            return type;
        }

        // The engine actually manages a CRC table of the classification names,
        // but since we uppercase the name and this feature is nowhere used,
        // except for "MultiplayerStructureMarker",
        // we can just use the name as the classification.
        var classificationName = GetTagName(element).ToUpperInvariant();
        var gameObjectType =  new GameObjectType(name, classificationName, nameCrc, parsedEntries.ValueCount, location);
        if (Logger != null)
            LogCreatingNewGameObjectType(Logger, gameObjectType.Name);
        return gameObjectType;
    }

    protected override void ParseObject(
        GameObjectType xmlObject, 
        XElement element, 
        bool replace,
        in IReadOnlyFrugalValueListDictionary<Crc32, GameObjectType> parsedEntries)
    {
        if (element.HasElements && element.Attribute("SubObjectList")?.Value == "Yes")
        {
            // TODO
            return;
        }
        
        if (OverlayLoad)
        {
            OverlayType(xmlObject, element, parsedEntries);
        }
        else
        {
            base.ParseObject(xmlObject, element, replace, in parsedEntries);
        }
    }

    protected override void ValidateAndFixupValues(GameObjectType gameObjectType, XElement element, in IReadOnlyFrugalValueListDictionary<Crc32, GameObjectType> parsedEntries)
    {
        if (!OverlayLoad)
        {
            // TODO
            //BehaviorClass.AddImpliedBehaviors(this, BehaviorNames);
            //InitBehaviorMap();

            PostLoadFixup(gameObjectType);
            if (string.IsNullOrEmpty(gameObjectType.VariantOfExistingTypeName))
                gameObjectType.IsLoadingComplete = true;
            else
                OverlayType(gameObjectType, element, parsedEntries);
        }
    }

    private void OverlayType(GameObjectType gameObjectType, XElement element, IReadOnlyFrugalValueListDictionary<Crc32, GameObjectType> parsedEntries)
    { 
        var baseType = gameObjectType.VariantOfExistingType;
        if (baseType is null)
        {
            var baseTypeName = gameObjectType.VariantOfExistingTypeName;
            if (string.IsNullOrEmpty(baseTypeName))
                return;

            var nameCrc = CreateNameCrc(baseTypeName);

            parsedEntries.TryGetFirstValue(nameCrc, out baseType);
            if (baseType is null)
                return;
        }
        OverlayType(baseType, gameObjectType, element);
    }

    private void OverlayType(GameObjectType baseType, GameObjectType derivedType, XElement element)
    {
        if (!baseType.IsLoadingComplete)
            return;

        derivedType.ApplyBaseType(baseType);

        ParseTags(derivedType, element, true, ReadOnlyFrugalValueListDictionary<Crc32, GameObjectType>.Empty);

        PostLoadFixup(derivedType);
        derivedType.IsLoadingComplete = true;
    }

    protected override bool ParseTag(
        XElement tag, 
        GameObjectType xmlObject,
        bool replace,
        in IReadOnlyFrugalValueListDictionary<Crc32, GameObjectType> parseState)
    {
        // The engine ignores the return value, but we do not, so we can report unknown tags.
        base.ParseTag(tag, xmlObject, replace, in parseState);

        // TODO: Once parsing is complete, return original parse result.
        return true;
    }

    private void PostLoadFixup(GameObjectType gameObjectType)
    {
        // TODO:
        // MaxSpeed *= 1.0;
        // MaxThrust *= 1.0;
        // Asserts and some coercions

        // The engine loads references for scripts, images, hardpoints, etc.,
        // but we don't do that here.
    }

    private sealed class GameObjectXmlTagMapper(IServiceProvider serviceProvider) : XmlTagMapper<GameObjectType>(serviceProvider)
    {
        protected override void BuildMappings()
        {
            AddMapping(
                GameObjectTypeXmlTags.GalacticModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GalacticModel = val);
            AddMapping(
                GameObjectTypeXmlTags.GalacticFleetOverrideModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GalacticFleetOverrideModel = val);
            AddMapping(
                GameObjectTypeXmlTags.DestroyedGalacticModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.DestroyedGalacticModel = val);
            AddMapping(
                GameObjectTypeXmlTags.LandModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.LandModel = val);
            AddMapping(
                GameObjectTypeXmlTags.LandTerrainModelMapping,
                CommaSeparatedStringKeyValueListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.InternalLandTerrainModelMapping, val, replace));
            AddMapping(
                GameObjectTypeXmlTags.SpaceModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.SpaceModel = val);
            AddMapping(
                GameObjectTypeXmlTags.LandModelAnimOverrideName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.LandAnimOverrideModel = val);
            AddMapping(
                GameObjectTypeXmlTags.SpaceModelAnimOverrideName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.SpaceAnimOverrideModel = val);

            AddMapping(
                GameObjectTypeXmlTags.CompanyUnits,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                // The MULTI_OBJECT_REFERENCE parser never replaces (this is done by the MultiReferenceList itself)
                (obj, val) => obj.GroundCompanyUnits.AddRange(val));

            AddMapping(
                GameObjectTypeXmlTags.DamagedSmokeAssetName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.DamagedSmokeAssetModel = val);

            
            AddMapping(
                GameObjectTypeXmlTags.GuiModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GuiModel = val);

            AddMapping(
                GameObjectTypeXmlTags.IconName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.IconName = val);

            AddMapping(
                GameObjectTypeXmlTags.VariantOfExistingType,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.VariantOfExistingTypeName = val);
        }
    }

    [LoggerMessage(LogLevel.Debug, "--- Creating new GameObjectTypeClass for key '{objectName}'")]
    static partial void LogCreatingNewGameObjectType(ILogger logger, string objectName);
}