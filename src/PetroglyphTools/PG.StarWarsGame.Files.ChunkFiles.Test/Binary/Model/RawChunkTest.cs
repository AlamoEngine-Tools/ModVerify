using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class RawChunkTest
{
    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 3);
        var chunk = new RawChunk(info, data);

        Assert.Equal(info, chunk.Info);
        Assert.Equal(data, chunk.Data.ToArray());
    }

    [Fact]
    public void Ctor_AllowsBit31Set()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 0x8000_0003u);
        var chunk = new RawChunk(info, data);
        Assert.True(chunk.Info.HasChildrenHint);
    }

    [Fact]
    public void Ctor_AllowsEmptyData()
    {
        var info = new ChunkMetadata(0x10, 0);
        var chunk = new RawChunk(info, ReadOnlyMemory<byte>.Empty);
        Assert.Equal(0, chunk.Data.Length);
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 5);
        Assert.Throws<ArgumentException>(() => new RawChunk(info, data));
    }

    [Fact]
    public void Size_IncludesHeaderAndData()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new ChunkMetadata(0x10, 3);
        var chunk = new RawChunk(info, data);
        Assert.Equal(8 + 3, chunk.Size);
    }

    [Fact]
    public void GetBytes_EmptyData_WritesHeaderOnly()
    {
        var info = new ChunkMetadata(0x03, 0);
        var chunk = new RawChunk(info, ReadOnlyMemory<byte>.Empty);

        var bytes = chunk.Bytes;
        Assert.Equal(8, bytes.Length);

        // Type (LE)
        Assert.Equal(0x03, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x00, bytes[2]);
        Assert.Equal(0x00, bytes[3]);

        // Size = 0 (LE)
        Assert.Equal(0x00, bytes[4]);
        Assert.Equal(0x00, bytes[5]);
        Assert.Equal(0x00, bytes[6]);
        Assert.Equal(0x00, bytes[7]);
    }

    [Fact]
    public void GetBytes_WritesRawSizeIncludingBit31()
    {
        var data = new byte[] { 0xCC };
        var info = new ChunkMetadata(0x02, 0x8000_0001u);
        var chunk = new RawChunk(info, data);

        var bytes = chunk.Bytes;
        Assert.Equal(9, bytes.Length);

        // Type (LE)
        Assert.Equal(0x02, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x00, bytes[2]);
        Assert.Equal(0x00, bytes[3]);

        // RawSize (LE) - bit 31 preserved
        Assert.Equal(0x01, bytes[4]);
        Assert.Equal(0x00, bytes[5]);
        Assert.Equal(0x00, bytes[6]);
        Assert.Equal(0x80, bytes[7]);

        // Data
        Assert.Equal(0xCC, bytes[8]);
    }

    [Fact]
    public void IsRootChunk()
    {
        var data = new byte[] { 1 };
        var info = new ChunkMetadata(1, 1);
        var chunk = new RawChunk(info, data);
        Assert.IsAssignableFrom<RootChunk>(chunk);
    }
}
