using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using PG.StarWarsGame.Files.TED.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace PG.StarWarsGame.Files.TED.Binary.Reader;

internal sealed class TedFileReader(TedLoadOptions loadOptions, Stream stream) 
    : ChunkFileReaderBase<IMapData>(stream), ITedFileReader
{
    protected TedLoadOptions LoadOptions { get; } = loadOptions;

    public override IMapData Read()
    {
        ChunkMetadata? chunk;
        MapInfo? mapInfo = null;

        byte[]? previewImageData = null;
        int? previewImageDataSize = null;
        List<Vector2> startPositionMarkers = [];

        var chunkCount = 0;
        while ((chunk = ChunkReader.TryReadChunk()) is not null)
        {
            if (chunkCount++ == 0 && chunk.Value.Type != (uint)TedChunkType.MapInfo)
                throw new BinaryCorruptedException($"The first chunk of a TED file must be of type {TedChunkType.MapInfo}.");
            
            switch ((TedChunkType)chunk.Value.Type)
            {
                case TedChunkType.MapInfo:
                    mapInfo = ReadMapInformation(chunk.Value);
                    break;
                case TedChunkType.StartPositions:
                    ReadStartPositionMarkers(chunk.Value, startPositionMarkers);
                    break;
                case TedChunkType.MapPreviewImageData:
                    ReadMapPreviewImageData(chunk.Value, out previewImageDataSize, out previewImageData);
                    break;
                default:
                    throw new BinaryCorruptedException($"Unknown chunk type: {chunk.Value.Type}.");
            }
        }

        if (mapInfo is null)
            throw new BinaryCorruptedException("The TED file does not have map information data.");
        
        mapInfo = mapInfo with
        {
            EmbeddedPreviewTextureFile = previewImageData,
            EmbeddedPreviewTextureFileSize = previewImageDataSize,
            StartPositionMarkers = startPositionMarkers
        };
        
        return new MapData(mapInfo);
    }

    private void ReadStartPositionMarkers(ChunkMetadata chunk, List<Vector2> startPositionMarkers)
    {
        var readBytes = 0;
        var positionsCount = (int)ChunkReader.ReadDword(ref readBytes);
        
        startPositionMarkers.Clear();

        for (var i = 0; i < positionsCount; i++)
        {
            var x = ChunkReader.ReadFloat(ref readBytes);
            var y = ChunkReader.ReadFloat(ref readBytes);
            startPositionMarkers.Add(new Vector2(x, y));
        }
        
        if (readBytes != chunk.BodySize)
            throw new BinaryCorruptedException(
                $"Unable to read Start Position Markers chunk. Expected {chunk.BodySize} bytes, but read {readBytes} bytes.");
    }

    private void ReadMapPreviewImageData(ChunkMetadata chunk, out int? previewImageDataSize, out byte[]? previewImageData)
    {
        Debug.Assert(!chunk.HasChildrenHint);

        previewImageDataSize = chunk.BodySize;

        if (LoadOptions.HasFlag(TedLoadOptions.PreviewImageData))
        {
            previewImageData = ChunkReader.ReadData(chunk);
            return;
        }

        ChunkReader.Skip(chunk.BodySize);
        previewImageData = null;
    }

    private MapInfo ReadMapInformation(ChunkMetadata chunk)
    {
        var readBytes = 0;

        var mapFileName = FileName ?? "[NO FILE NAME]";
        int version = 0;
        var type = -1;
        var playerCount = 1;
        var mapLevels = 1;
        var supportedWeather = 1;
        var factionOwner = 0;
        var mapType = 0;
        var supportedSystems = 0;
        var mapName = string.Empty;
        var planetName = string.Empty;
        var gameTypes = string.Empty;
        var customMap = false;
        var width = 0.0f;
        var height = 0.0f;
        var newMultiplayerMarkerSystem = false;

        MiniChunkMetadata? miniChunk;
        while ((miniChunk = ChunkReader.ReadMiniChunk(ref readBytes)) is not null)
        {
            switch ((MapInfoChunkType)miniChunk.Value.Type)
            {
                case MapInfoChunkType.Version:
                    version = (int)ChunkReader.ReadDword(ref readBytes);
                    Debug.Assert(version == 513); // 01 02 00 00 (LE)
                    break;
            }
        }
        
        if (version == 0)
            throw new BinaryCorruptedException("Map Information chunk is missing the version mini-chunk.");

        if (readBytes != chunk.BodySize)
            throw new BinaryCorruptedException(
                $"Unable to read Map Information chunk. Expected {chunk.BodySize} bytes, but read {readBytes} bytes.");

        if (string.IsNullOrEmpty(mapName))
            mapName = mapFileName;

        return new MapInfo
        {
            MapFileName = mapFileName,
            Version = version,
            Type = (MapMode)type,
            MaxPlayers = playerCount,
            Levels = mapLevels,
            Terrain = (MapEnvironmentType)mapType,
            Owner = (MapOwnerType)factionOwner,
            MapName = mapName,
            PlanetName = planetName,
            GameTypes = gameTypes.ToUpper(), // The game actually uses the current locale
            CustomMap = customMap,
            MapSize = new Vector2(width, height),
            NewMultiplayerMarkerSystem = newMultiplayerMarkerSystem
        };
    }
}


[Flags]
public enum TedLoadOptions
{
    /// <summary>
    /// Loads the entire file.
    /// </summary>  
    Full = 0,
    PreviewImageData = 1,
    XRefs = 2
}