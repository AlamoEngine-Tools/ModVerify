using System;
using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public sealed class GameObject : XmlObject
{
    internal GameObject(
        string type, 
        string name, 
        Crc32 nameCrc, 
        GameObjectType estimatedType, 
        IReadOnlyValueListDictionary<string, object?> properties, 
        XmlLocationInfo location) 
        : base(name, nameCrc, properties, location)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        EstimatedType = estimatedType;
    }

    public string Type { get; }


    public GameObjectType EstimatedType { get; }

    /// <summary>
    /// Gets all model files (including particles) the game object references.
    /// </summary>
    public ISet<string> Models
    {
        get
        {
            var models = XmlProperties.AggregateValues<string, object?, string>
            (new HashSet<string>
                {
                    "Galactic_Model_Name",
                    "Destroyed_Galactic_Model_Name",
                    "Land_Model_Name",
                    "Space_Model_Name",
                    "Model_Name",
                    "Tactical_Model_Name",
                    "Galactic_Fleet_Override_Model_Name",
                    "GUI_Model_Name",
                    "GUI_Model",
                    // This can either be a model or a unit reference
                    "Land_Model_Anim_Override_Name",
                    "xxxSpace_Model_Name",
                    "Damaged_Smoke_Asset_Name"
                }, v => v.EndsWith(".alo", StringComparison.OrdinalIgnoreCase),
                ValueListDictionaryExtensions.AggregateStrategy.LastValuePerKey);

            var terrainMappedModels = LandTerrainModelMapping?.Select(x => x.Model);
            if (terrainMappedModels is null)
                return new HashSet<string>(models, StringComparer.OrdinalIgnoreCase);

            return new HashSet<string>(models.Concat(terrainMappedModels), StringComparer.OrdinalIgnoreCase);
        }
    }

    public IList<(string Terrain, string Model)>? LandTerrainModelMapping =>
        GetLastPropertyOrDefault<IList<(string Terrain, string Model)>>("Land_Terrain_Model_Mapping");
}