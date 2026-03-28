using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PG.StarWarsGame.Files.Binary.File;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

public sealed class ChunkFile : IBinaryFile
{
    public IReadOnlyList<Chunk> RootChunks { get; }

    public int Size => RootChunks.Sum(c => c.Size);

    public byte[] Bytes
    {
        get
        {
            var bytes = new byte[Size];
            GetBytes(bytes);
            return bytes;
        }
    }

    public ChunkFile(IReadOnlyList<Chunk> rootChunks)
    {
        if (rootChunks == null) 
            throw new ArgumentNullException(nameof(rootChunks));
        if (rootChunks.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(rootChunks), "A chunk file must contain at least one chunk");
        RootChunks = rootChunks;
    }

    public void GetBytes(Span<byte> bytes)
    {
        var offset = 0;
        foreach (var chunk in RootChunks)
        {
            chunk.GetBytes(bytes[offset..]);
            offset += chunk.Size;
        }
    }

    public void WriteTo(Stream stream)
    {
        stream.Write(Bytes, 0, Size);
    }
}