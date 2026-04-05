using System;
using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

namespace PG.StarWarsGame.Files.TED.Binary.Writer;

internal sealed class MapPreviewExtractor
{
    public bool ContainsPreviewPicture(Stream tedStream)
    {
        if (tedStream == null) 
            throw new ArgumentNullException(nameof(tedStream));
        using var reader = new ChunkReader(tedStream, true);
        ChunkMetadata? chunkInfo;
        while ((chunkInfo = reader.TryReadChunk()) != null)
        { 
            if (chunkInfo.Value.Type == 0x13)
                return true;
            reader.Skip(chunkInfo.Value.BodySize);
        }
        return false;
    }

    public bool ExtractPreview(Stream tedStream, Stream destination, bool extract, out byte[]? previewImageBytes)
    {
        if (tedStream == null) 
            throw new ArgumentNullException(nameof(tedStream));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        previewImageBytes = null;
        var extracted = false;

        using var reader = new ChunkReader(tedStream, true);

        ChunkMetadata? chunkInfo;
        while ((chunkInfo = reader.TryReadChunk()) != null)
        {
            if (chunkInfo.Value.Type == 0x13)
            {
                extracted = true;
                if (extract) 
                    previewImageBytes = reader.ReadData(chunkInfo.Value.BodySize);
                else
                    reader.Skip(chunkInfo.Value.BodySize);
            }
            else
            {
                var chunk = new RawChunk(chunkInfo.Value, reader.ReadData(chunkInfo.Value.BodySize));
                destination.Write(chunk.Bytes, 0, chunk.Bytes.Length);
            }
        }

        return extracted;
    }
}