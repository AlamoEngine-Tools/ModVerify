using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.GameObjects;
using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
using PG.StarWarsGame.Engine.Xml.Parsers.Tags;
using PG.StarWarsGame.Files.ALO.Data;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyModels(GameObjectType gameObjectType, string[] context, CancellationToken token)
    {
        // TODO: These tags support both models and particles, depending on configuration such as Behavior.
        // For now, we just verify they are not animations, but we want to perform an actual sanity check later
        if (!string.IsNullOrEmpty(gameObjectType.GalacticModel))
        {
            var model = _singleModelVerifier.VerifyAlamoFile(gameObjectType.GalacticModel,
                [..context, $"Tag: {GameObjectTypeXmlTags.GalacticModelName}"], token);
            
            if (model?.RenderableContent is AlamoAnimation)
            {
                AddError(Diagnostics.GameObjects.ExpectedModelOrParticle(this, "galactic model", gameObjectType.Name,
                    model.File.FileName.ToUpperInvariant(), [..context, $"Tag: {GameObjectTypeXmlTags.GalacticModelName}"]));
            }
        }
        if (!string.IsNullOrEmpty(gameObjectType.LandModel))
        {
            var model = _singleModelVerifier.VerifyAlamoFile(gameObjectType.LandModel,
                [.. context, $"Tag: {GameObjectTypeXmlTags.LandModelName}"], token);

            if (model?.RenderableContent is AlamoAnimation)
            {
                AddError(Diagnostics.GameObjects.ExpectedModelOrParticle(this, "land model", gameObjectType.Name,
                    model.File.FileName.ToUpperInvariant(), [.. context, $"Tag: {GameObjectTypeXmlTags.LandModelName}"]));
            }
        }
        if (!string.IsNullOrEmpty(gameObjectType.SpaceModel))
        {
            var model = _singleModelVerifier.VerifyAlamoFile(gameObjectType.SpaceModel,
                [.. context, $"Tag: {GameObjectTypeXmlTags.SpaceModelName}"], token);

            if (model?.RenderableContent is AlamoAnimation)
            {
                AddError(Diagnostics.GameObjects.ExpectedModelOrParticle(this, "space model", gameObjectType.Name,
                    model.File.FileName.ToUpperInvariant(), [.. context, $"Tag: {GameObjectTypeXmlTags.SpaceModelName}"]));
            }
        }
        if (!string.IsNullOrEmpty(gameObjectType.DamagedSmokeAssetModel))
        {
            var model = _singleModelVerifier.VerifyAlamoFile(gameObjectType.DamagedSmokeAssetModel,
                [.. context, $"Tag: {GameObjectTypeXmlTags.DamagedSmokeAssetName}"], token);

            if (model?.RenderableContent is AlamoAnimation)
            {
                AddError(Diagnostics.GameObjects.ExpectedModelOrParticle(this, "damaged smoke asset", gameObjectType.Name,
                    model.File.FileName.ToUpperInvariant(), [.. context, $"Tag: {GameObjectTypeXmlTags.DamagedSmokeAssetName}"]));
            }
        }

        if (!string.IsNullOrEmpty(gameObjectType.DestroyedGalacticModel)) 
            _singleModelVerifier.VerifyModel(gameObjectType.DestroyedGalacticModel, [..context, $"Tag: {GameObjectTypeXmlTags.DestroyedGalacticModelName}"], token);
        if (!string.IsNullOrEmpty(gameObjectType.GalacticFleetOverrideModel))
            _singleModelVerifier.VerifyModel(gameObjectType.GalacticFleetOverrideModel, [..context, $"Tag: {GameObjectTypeXmlTags.GalacticFleetOverrideModelName}"], token);
        if (!string.IsNullOrEmpty(gameObjectType.GuiModel))
            _singleModelVerifier.VerifyModel(gameObjectType.GuiModel, [..context, $"Tag: {GameObjectTypeXmlTags.GuiModelName}"], token);
        if (!string.IsNullOrEmpty(gameObjectType.LandAnimOverrideModel))
            _singleModelVerifier.VerifyModel(gameObjectType.LandAnimOverrideModel, [..context, $"Tag: {GameObjectTypeXmlTags.LandModelAnimOverrideName}"], token);
        if (!string.IsNullOrEmpty(gameObjectType.SpaceAnimOverrideModel))
            _singleModelVerifier.VerifyModel(gameObjectType.SpaceAnimOverrideModel, [..context, $"Tag: {GameObjectTypeXmlTags.SpaceModelAnimOverrideName}"], token);
        

        var terrainModelMapping = GetLandTerrainModelMapping(gameObjectType, out var invalidTerrainTypes);

        foreach (var mapping in terrainModelMapping)
        { 
            _singleModelVerifier.VerifyModel(mapping.Value.First(), 
                [..context, $"Tag: {GameObjectTypeXmlTags.LandTerrainModelMapping}", $"Terrain: {mapping.Key}"], token);

            // If there are multiple models defined for the same terrain type,
            // log a warning since only the first one will be used by the engine.
            // This is likely an unintentional mistake in the XML.
            if (mapping.Value.Count > 1)
            {
                AddError(Diagnostics.GameObjects.DuplicateTerrainMapping(this, mapping.Key.ToString(), gameObjectType.Name, context));
            }
        }

        foreach (var invalidTerrain in invalidTerrainTypes)
        {
            // Error, since this indicates a likely typo in the XML that results in the terrain type being ignored,
            // which can lead to missing models in-game and is likely not intentional.
            AddError(Diagnostics.GameObjects.InvalidTerrainType(this, invalidTerrain,
                GameObjectTypeXmlTags.LandTerrainModelMapping, gameObjectType.Name, [..context, nameof(MapEnvironmentType)]));
        }
    }
    
    // NB: The engine uses the first model it finds for a given terrain type.
    private static ReadOnlyFrugalValueListDictionary<MapEnvironmentType, string> GetLandTerrainModelMapping(
        GameObjectType gameObjectType, 
        out IReadOnlyCollection<string> invalidTerrains)
    {
        if (gameObjectType.LandTerrainModelMappingValues.Count == 0)
        {
            invalidTerrains = [];
            return ReadOnlyFrugalValueListDictionary<MapEnvironmentType, string>.Empty;
        }   

        var dict = new FrugalValueListDictionary<MapEnvironmentType, string>();
        var invalidList = new List<string>();

        foreach (var mapping in gameObjectType.LandTerrainModelMappingValues)
        {
            if (!MapEnvironmentTypeConversion.ConversionDictionary.TryStringToEnum(mapping.terrainValue, out var terrain))
            {
                invalidList.Add(mapping.terrainValue);
                continue;
            }
            dict.Add(terrain, mapping.model);
        }

        invalidTerrains = invalidList;
        return new ReadOnlyFrugalValueListDictionary<MapEnvironmentType, string>(dict);
    }
}