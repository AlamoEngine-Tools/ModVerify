using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Engine.GameObjects;

[DebuggerDisplay("{Name} ({ClassificationName})")]
public sealed class GameObject : NamedXmlObject
{
    internal readonly List<(string terrain, string model)> InternalLandTerrainModelMapping = [];

    internal int Id { get; }

    public int Index { get; }

    public string ClassificationName { get; }

    public string? VariantOfExistingTypeName { get; internal set; }

    public GameObject? VariantOfExistingType { get; internal set; }

    public bool IsLoadingComplete { get; internal set; }
    
    public string? GalacticModel { get; internal set; }

    public string? DestroyedGalacticModel { get; internal set; }

    public string? LandModel { get; internal set; }

    public string? SpaceModel { get; internal set; }

    public string? GalacticFleetOverrideModel { get; internal set; }

    public string? GuiModel { get; internal set; }

    public string? ModelName { get; internal set; }

    public string? LandAnimOverrideModel { get; internal set; }

    public string SpaceAnimOverrideModel { get; internal set; }

    public string? DamagedSmokeAssetModel { get; internal set; }

    public IReadOnlyList<(string terrain, string model)> LandTerrainModelMappingValues { get; }
    
    internal GameObject(
        string name,
        string classification,
        Crc32 nameCrc,
        int index,
        XmlLocationInfo location)
        : base(name, nameCrc, location)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than 0.");
        Index = index;
        Id = (int)nameCrc;
        ClassificationName = classification ?? throw new ArgumentNullException(nameof(classification));
        LandTerrainModelMappingValues = new ReadOnlyCollection<(string, string)>(InternalLandTerrainModelMapping);
    }

    internal void ApplyBaseType(GameObject baseType)
    {
        // The following properties must not be inherited from the base type:
        // ID, CRC, Name, Location, IsLoadingComplete, ClassificationName and VariantOfExistingType[Name], LuaScript

        GalacticModel = baseType.GalacticModel;
        DestroyedGalacticModel = baseType.DestroyedGalacticModel;
        LandModel = baseType.LandModel;
        SpaceModel = baseType.SpaceModel;
        GalacticFleetOverrideModel = baseType.GalacticFleetOverrideModel;
        GuiModel = baseType.GuiModel;
        ModelName = baseType.ModelName;
        LandAnimOverrideModel = baseType.LandAnimOverrideModel;
        SpaceAnimOverrideModel = baseType.SpaceAnimOverrideModel;
        DamagedSmokeAssetModel = baseType.DamagedSmokeAssetModel;
        InternalLandTerrainModelMapping.ClearAddRange(baseType.InternalLandTerrainModelMapping);
    }

    internal void PostLoadFixup()
    {
        // This method is different than the fixup that is performed after parsing the object.
        // IDK why there are two separate fixups. This fixup performs more value coercions
    }
}