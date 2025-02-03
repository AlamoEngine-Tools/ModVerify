using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.GameObjects;

public sealed class GameObject : NamedXmlObject
{
    internal GameObject(string type, string name, Crc32 nameCrc, GameObjectType estimatedType, XmlLocationInfo location) 
        : base(name, nameCrc, location)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        EstimatedType = estimatedType;
        LandTerrainModelMapping = new ReadOnlyDictionary<string, string>(InternalLandTerrainModelMapping);
    }

    public string Type { get; }

    public GameObjectType EstimatedType { get; }

    public string? GalacticModel { get; internal set; }

    public string? DestroyedGalacticModel { get; internal set; }

    public string? LandModel { get; internal set; }

    public string? SpaceModel { get; internal set; }

    public string? TacticalModel { get; internal set; }

    public string? GalacticFleetOverrideModel { get; internal set; }

    public string? GuiModel { get; internal set; }

    public string? ModelName { get; internal set; }

    public string? LandAnimOverrideModel { get; internal set; }

    public string? XxxSpaceModeModel { get; internal set; }

    public string? DamagedSmokeAssetModel { get; internal set; }

    public IReadOnlyDictionary<string, string> LandTerrainModelMapping { get; }

    internal Dictionary<string, string> InternalLandTerrainModelMapping { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all model files (including particles) the game object references.
    /// </summary>
    public IEnumerable<string> Models
    {
        get
        {
            var models = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddNotEmpty(models, GalacticModel);
            AddNotEmpty(models, DestroyedGalacticModel);
            AddNotEmpty(models, LandModel);
            AddNotEmpty(models, SpaceModel);
            AddNotEmpty(models, TacticalModel);
            AddNotEmpty(models, GalacticFleetOverrideModel);
            AddNotEmpty(models, GuiModel);
            AddNotEmpty(models, ModelName);
            AddNotEmpty(models, LandAnimOverrideModel, s => s.EndsWith(".alo", StringComparison.OrdinalIgnoreCase));
            AddNotEmpty(models, XxxSpaceModeModel);
            AddNotEmpty(models, DamagedSmokeAssetModel);
            foreach (var model in InternalLandTerrainModelMapping.Values) 
                models.Add(model);

            return models;
        }
        
    }

    private static void AddNotEmpty(ISet<string> set, string? value, Predicate<string>? predicate = null)
    {
        if (value is null) 
            return;
        if (predicate is null || predicate(value)) 
            set.Add(value);
    }
}