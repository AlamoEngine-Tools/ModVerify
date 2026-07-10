using System;
using System.Collections.Generic;
using System.Numerics;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.ChunkFiles.Data;

namespace PG.StarWarsGame.Files.TED.Data;

public interface IMapData : IChunkData
{
    public MapInfo MapInfo { get; }
}

public class MapData : DisposableObject, IMapData
{
    public MapInfo MapInfo { get; }

    public MapData(MapInfo mapInfo)
    {
        MapInfo = mapInfo ?? throw new ArgumentNullException(nameof(mapInfo));
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        MapInfo.Dispose();
    }
}

public sealed record MapInfo : IDisposable
{
    private byte[]? _previewImage;
    
    public int Version { get; init; }

    public string MapFileName
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public string MapName
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public string DisplayName => $"({MaxPlayers}) {MapName}";

    public string PlanetName
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public string GameTypes 
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public MapMode Type { get; init; }

    public MapEnvironmentType Terrain { get; init; }

    public MapOwnerType Owner { get; init; }

    public int MaxPlayers { get; init; }

    public int Levels { get; init; }

    public bool CustomMap { get; init; }

    public bool NewMultiplayerMarkerSystem { get; init; }
    
    public Vector2 MapSize { get; init; }

    public IReadOnlyList<Vector2> StartPositionMarkers
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = [];

    public byte[]? EmbeddedPreviewTextureFile
    {
        get
        {
            if (_previewImage is null)
                return null;
            return (byte[])_previewImage.Clone();
        }
        init => _previewImage = value;
    }
    
    public int? EmbeddedPreviewTextureFileSize { get; init; }

    // We use the presence of the file size to determine if a preview image exists,
    // as the file data itself may not have been loaded.
    public bool PreviewImageExists => EmbeddedPreviewTextureFileSize.HasValue;
    
    public void Dispose()
    {
        if (_previewImage is not null)
        {
            Array.Clear(_previewImage, 0, _previewImage.Length);
            _previewImage = null;
        }
    }
}


// TODO: What happens if a map has 0 (Galactic) set???
public enum MapMode
{
    Land = 1,
    Space = 2
}

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

public enum MapOwnerType
{
    Rebel = 0x0,
    Empire = 0x1,
    Pirate = 0x2,
    Branched = 0x3,
    Underworld = 0x4,
    Hutts = 0x5,
}