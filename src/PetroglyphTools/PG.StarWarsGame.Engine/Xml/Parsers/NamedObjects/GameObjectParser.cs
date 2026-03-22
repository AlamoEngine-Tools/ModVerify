using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using Crc32 = PG.Commons.Hashing.Crc32;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal partial class GameObjectParser(
    GameEngineType engine,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : NamedXmlObjectParser<GameObject>(engine, new GameObjectXmlTagMapper(serviceProvider), errorReporter, serviceProvider)
{
    internal bool OverlayLoad { get; set; }

    protected override bool UpperCaseNameForCrc => true;
    
    protected override bool UpperCaseNameForObject => true;

    protected override GameObject CreateXmlObject(
        string name, 
        Crc32 nameCrc, 
        XElement element, 
        IReadOnlyFrugalValueListDictionary<Crc32, GameObject> parsedEntries, 
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
        var gameObjectType =  new GameObject(name, classificationName, nameCrc, parsedEntries.ValueCount, location);
        if (Logger != null)
            LogCreatingNewGameObjectType(Logger, gameObjectType.Name);
        return gameObjectType;
    }

    protected override void ParseObject(
        GameObject xmlObject, 
        XElement element, 
        bool replace,
        in IReadOnlyFrugalValueListDictionary<Crc32, GameObject> parsedEntries)
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

    protected override void ValidateAndFixupValues(GameObject gameObject, XElement element, in IReadOnlyFrugalValueListDictionary<Crc32, GameObject> parsedEntries)
    {
        if (!OverlayLoad)
        {
            // TODO
            //BehaviorClass.AddImpliedBehaviors(this, BehaviorNames);
            //InitBehaviorMap();

            PostLoadFixup(gameObject);
            if (string.IsNullOrEmpty(gameObject.VariantOfExistingTypeName))
                gameObject.IsLoadingComplete = true;
            else
                OverlayType(gameObject, element, parsedEntries);
        }
    }

    private void OverlayType(GameObject gameObject, XElement element, IReadOnlyFrugalValueListDictionary<Crc32, GameObject> parsedEntries)
    { 
        var baseType = gameObject.VariantOfExistingType;
        if (baseType is null)
        {
            var baseTypeName = gameObject.VariantOfExistingTypeName;
            if (string.IsNullOrEmpty(baseTypeName))
                return;

            var nameCrc = CreateNameCrc(baseTypeName);

            parsedEntries.TryGetFirstValue(nameCrc, out baseType);
            if (baseType is null)
                return;
        }
        OverlayType(baseType, gameObject, element);
    }

    private void OverlayType(GameObject baseType, GameObject derivedType, XElement element)
    {
        if (!baseType.IsLoadingComplete)
            return;

        derivedType.ApplyBaseType(baseType);

        ParseTags(derivedType, element, true, ReadOnlyFrugalValueListDictionary<Crc32, GameObject>.Empty);

        PostLoadFixup(derivedType);
        derivedType.IsLoadingComplete = true;
    }

    protected override bool ParseTag(
        XElement tag, 
        GameObject xmlObject,
        bool replace,
        in IReadOnlyFrugalValueListDictionary<Crc32, GameObject> parseState)
    {
        // The engine ignores the return value, but we do not, so we can report unknown tags.
        base.ParseTag(tag, xmlObject, replace, in parseState);

        // TODO: Once parsing is complete, return original parse result.
        return true;
    }

    private void PostLoadFixup(GameObject gameObject)
    {
        // TODO:
        // MaxSpeed *= 1.0;
        // MaxThrust *= 1.0;
        // Asserts and some coercions

        // The engine loads references for scripts, images, hardpoints, etc.,
        // but we don't do that here.
    }

    private sealed class GameObjectXmlTagMapper(IServiceProvider serviceProvider) : XmlTagMapper<GameObject>(serviceProvider)
    {
        protected override void BuildMappings()
        {
            AddMapping(
                GameObjectXmlTags.GalacticModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GalacticModel = val);
            AddMapping(
                GameObjectXmlTags.GalacticFleetOverrideModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GalacticFleetOverrideModel = val);
            AddMapping(
                GameObjectXmlTags.DestroyedGalacticModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.DestroyedGalacticModel = val);
            AddMapping(
                GameObjectXmlTags.LandModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.LandModel = val);
            AddMapping(
                GameObjectXmlTags.LandTerrainModelMapping,
                CommaSeparatedStringKeyValueListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.InternalLandTerrainModelMapping, val, replace));
            AddMapping(
                GameObjectXmlTags.SpaceModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.SpaceModel = val);
            AddMapping(
                GameObjectXmlTags.LandModelAnimOverrideName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.LandAnimOverrideModel = val);
            AddMapping(
                GameObjectXmlTags.SpaceModelAnimOverrideName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.SpaceAnimOverrideModel = val);

            AddMapping(
                GameObjectXmlTags.CompanyUnits,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                // The MULTI_OBJECT_REFERENCE parser never replaces (this is done by the MultiReferenceList itself)
                (obj, val) => obj.GroundCompanyUnits.AddRange(val));

            AddMapping(
                GameObjectXmlTags.DamagedSmokeAssetName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.DamagedSmokeAssetModel = val);

            
            AddMapping(
                GameObjectXmlTags.GuiModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.GuiModel = val);

            AddMapping(
                GameObjectXmlTags.IconName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.IconName = val);

            AddMapping(
                GameObjectXmlTags.VariantOfExistingType,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.VariantOfExistingTypeName = val);
        }
    }

    [LoggerMessage(LogLevel.Debug, "--- Creating new GameObjectTypeClass for key '{objectName}'")]
    static partial void LogCreatingNewGameObjectType(ILogger logger, string objectName);

    internal static class GameObjectXmlTags
    {
        public const string LandTerrainModelMapping = "Land_Terrain_Model_Mapping";
        public const string GalacticModelName = "Galactic_Model_Name";
        public const string DestroyedGalacticModelName = "Destroyed_Galactic_Model_Name";
        public const string LandModelName = "Land_Model_Name";
        public const string SpaceModelName = "Space_Model_Name";
        public const string GalacticFleetOverrideModelName = "Galactic_Fleet_Override_Model_Name";
        public const string GuiModelName = "GUI_Model_Name";
        public const string LandModelAnimOverrideName = "Land_Model_Anim_Override_Name";
        public const string SpaceModelAnimOverrideName = "Space_Model_Anim_Override_Name";
        public const string DamagedSmokeAssetName = "Damaged_Smoke_Asset_Name";

        public const string VariantOfExistingType = "Variant_Of_Existing_Type";

        public const string IconName = "Icon_Name";
        public const string CompanyUnits = "Company_Units";
    }
}