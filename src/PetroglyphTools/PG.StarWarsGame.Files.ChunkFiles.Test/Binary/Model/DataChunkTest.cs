using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class DataChunkTest
{
    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 3);
        var chunk = new DataChunk(info, data);

        Assert.Equal(info, chunk.Info);
        Assert.Equal(data, chunk.Data.ToArray());
    }

    [Fact]
    public void Ctor_ThrowsOnEmptyData()
    {
        var info = new ChunkMetadata(0x10, 0);
        Assert.Throws<ArgumentException>(() => new DataChunk(info, ReadOnlyMemory<byte>.Empty));
    }

    [Fact]
    public void Ctor_ThrowsWhenBit31Set()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 0x8000_0003u);
        Assert.Throws<ArgumentException>(() => new DataChunk(info, data));
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 5);
        Assert.Throws<ArgumentException>(() => new DataChunk(info, data));
    }

    [Fact]
    public void Size_IncludesHeaderAndData()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 3);
        var chunk = new DataChunk(info, data);

        // Header is 8 bytes (sizeof ChunkMetadata) + 3 bytes data
        Assert.Equal(8 + 3, chunk.Size);
    }

    [Fact]
    public void GetBytes_WritesCorrectBinary()
    {
        var data = new byte[] { 0xAA, 0xBB };
        var info = new ChunkMetadata(0x01, 2);
        var chunk = new DataChunk(info, data);

        var bytes = chunk.Bytes;
        Assert.Equal(10, bytes.Length);

        // Type (LE)
        Assert.Equal(0x01, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x00, bytes[2]);
        Assert.Equal(0x00, bytes[3]);

        // Size (LE) - no bit 31
        Assert.Equal(0x02, bytes[4]);
        Assert.Equal(0x00, bytes[5]);
        Assert.Equal(0x00, bytes[6]);
        Assert.Equal(0x00, bytes[7]);

        // Data
        Assert.Equal(0xAA, bytes[8]);
        Assert.Equal(0xBB, bytes[9]);
    }
}
