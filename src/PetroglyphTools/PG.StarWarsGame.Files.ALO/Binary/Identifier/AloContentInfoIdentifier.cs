using System.IO;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

namespace PG.StarWarsGame.Files.ALO.Binary.Identifier;

internal class AloContentInfoIdentifier : IAloContentInfoIdentifier
{
    public AloContentInfo GetContentInfo(Stream stream)
    {
        using var chunkReader = new ChunkReader(stream, true);

        var chunk = chunkReader.ReadChunk();

        switch ((AloChunkType)chunk.Type)
        {
            case AloChunkType.Skeleton:
            case AloChunkType.Mesh:
            case AloChunkType.Light:
                return FromModel(chunk.BodySize, chunkReader);
            case AloChunkType.Connections:
                return FromConnection(chunkReader);
            case AloChunkType.Particle:
                return new AloContentInfo(AloType.Particle, AloVersion.V1);
            case AloChunkType.ParticleUaW:
                return new AloContentInfo(AloType.Particle, AloVersion.V2);
            case AloChunkType.Animation:
                return FromAnimation(chunkReader);
            default:
                throw new BinaryCorruptedException("Unable to get ALO content information.");
        }
    }

    private static AloContentInfo FromAnimation(ChunkReader chunkReader)
    {
        var chunk = chunkReader.TryReadChunk();
        while (chunk.HasValue)
        {
            switch ((AloChunkType)chunk.Value.Type)
            {
                case AloChunkType.AnimationInformation:
                    return chunk.Value.BodySize switch
                    {
                        36 => new AloContentInfo(AloType.Animation, AloVersion.V2),
                        18 => new AloContentInfo(AloType.Animation, AloVersion.V1),
                        _ => throw new BinaryCorruptedException("Invalid ALA animation.")
                    };
                default:
                    chunkReader.Skip(chunk.Value.BodySize);
                    break;
            }
            chunk = chunkReader.TryReadChunk();
        }
        throw new BinaryCorruptedException("Invalid ALA animation.");
    }

    private static AloContentInfo FromConnection(ChunkReader chunkReader)
    {
        var chunk = chunkReader.TryReadChunk();
        while (chunk.HasValue)
        {
            switch ((AloChunkType)chunk.Value.Type)
            {
                case AloChunkType.ProxyConnection:
                case AloChunkType.ObjectConnection:
                case AloChunkType.ConnectionCounts:
                    chunkReader.Skip(chunk.Value.BodySize);
                    break;
                case AloChunkType.Dazzle:
                    return new AloContentInfo(AloType.Model, AloVersion.V2);
                default:
                    throw new BinaryCorruptedException("Invalid ALO model.");
            }
            chunk = chunkReader.TryReadChunk();
        }

        return new AloContentInfo(AloType.Model, AloVersion.V1);
    }

    private static AloContentInfo FromModel(int size, ChunkReader chunkReader)
    {
        chunkReader.Skip(size);
        var chunk = chunkReader.TryReadChunk();
        if (chunk is null) 
            throw new BinaryCorruptedException("Unable to get ALO content information.");
        switch ((AloChunkType)chunk.Value.Type)
        {
            case AloChunkType.Connections:
                return FromConnection(chunkReader);
            case AloChunkType.Skeleton:
            case AloChunkType.Mesh:
            case AloChunkType.Light:
                return FromModel(chunk.Value.BodySize, chunkReader);
            default:
                throw new BinaryCorruptedException("Invalid ALO model.");
        }
    }
}