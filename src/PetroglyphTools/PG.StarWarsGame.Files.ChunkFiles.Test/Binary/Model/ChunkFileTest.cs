using System;
using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class ChunkFileTest
{
    private static RawChunk CreateRoot(uint type, byte[] data)
    {
        return new RawChunk(new ChunkMetadata(type, (uint)data.Length), data);
    }

    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var root = CreateRoot(1, [0xAA]);
        var file = new ChunkFile([root]);

        Assert.Single(file.RootChunks);
        Assert.Same(root, file.RootChunks[0]);
    }

    [Fact]
    public void Ctor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ChunkFile(null!));
    }

    [Fact]
    public void Ctor_AllowsEmpty()
    {
        var file = new ChunkFile([]);
        Assert.Empty(file.RootChunks);
    }

    [Fact]
    public void Size_SumsRootChunkSizes()
    {
        var r1 = CreateRoot(1, [0xAA]);
        var r2 = CreateRoot(2, [0xBB, 0xCC]);
        var file = new ChunkFile([r1, r2]);

        Assert.Equal(r1.Size + r2.Size, file.Size);
    }

    [Fact]
    public void Bytes_ReturnsCorrectBinary()
    {
        var r1 = CreateRoot(1, [0xAA]);
        var file = new ChunkFile([r1]);

        var bytes = file.Bytes;
        Assert.Equal(file.Size, bytes.Length);
        Assert.Equal(r1.Bytes, bytes);
    }

    [Fact]
    public void GetBytes_WritesExactByteSequence()
    {
        // type=0x01, size=1: [0x01,0x00,0x00,0x00, 0x01,0x00,0x00,0x00, 0xAA]
        // type=0x02, size=2: [0x02,0x00,0x00,0x00, 0x02,0x00,0x00,0x00, 0xBB,0xCC]
        var r1 = CreateRoot(1, [0xAA]);
        var r2 = CreateRoot(2, [0xBB, 0xCC]);
        var file = new ChunkFile([r1, r2]);

        Assert.Equal(new byte[]
        {
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xAA,
            0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xBB, 0xCC,
        }, file.Bytes);
    }

    [Fact]
    public void WriteTo_WritesToStream()
    {
        var r1 = CreateRoot(1, [0xAA]);
        var file = new ChunkFile([r1]);

        using var ms = new MemoryStream();
        file.WriteTo(ms);

        Assert.Equal(file.Bytes, ms.ToArray());
    }

    [Fact]
    public void WriteTo_ThrowsOnNullStream()
    {
        var r1 = CreateRoot(1, [0xAA]);
        var file = new ChunkFile([r1]);

        Assert.Throws<ArgumentNullException>(() => file.WriteTo(null!));
    }
}
