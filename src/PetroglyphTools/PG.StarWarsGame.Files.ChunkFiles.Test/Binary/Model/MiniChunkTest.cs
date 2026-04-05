using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class MiniChunkTest
{
    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var data = new byte[] { 0xAA, 0xBB };
        var info = new MiniChunkMetadata(0x05, 2);
        var chunk = new MiniChunk(info, data);

        Assert.Equal(info, chunk.Info);
        Assert.Equal(data, chunk.Data.ToArray());
    }

    [Fact]
    public void Ctor_AllowsEmptyData()
    {
        var info = new MiniChunkMetadata(0x05, 0);
        var chunk = new MiniChunk(info, ReadOnlyMemory<byte>.Empty);
        Assert.Equal(0, chunk.Data.Length);
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new MiniChunkMetadata(0x05, 5);
        Assert.Throws<ArgumentException>(() => new MiniChunk(info, data));
    }

    [Fact]
    public void Size_IncludesHeaderAndData()
    {
        var data = new byte[] { 1, 2, 3 };
        var info = new MiniChunkMetadata(0x05, 3);
        var chunk = new MiniChunk(info, data);

        // Header is 2 bytes (sizeof MiniChunkMetadata) + 3 bytes data
        Assert.Equal(2 + 3, chunk.Size);
    }

    [Fact]
    public void GetBytes_EmptyData_WritesHeaderOnly()
    {
        var info = new MiniChunkMetadata(0x0B, 0);
        var chunk = new MiniChunk(info, ReadOnlyMemory<byte>.Empty);

        var bytes = chunk.Bytes;
        Assert.Equal(2, bytes.Length);

        Assert.Equal(0x0B, bytes[0]); // Type
        Assert.Equal(0x00, bytes[1]); // Size = 0
    }

    [Fact]
    public void GetBytes_WritesCorrectBinary()
    {
        var data = new byte[] { 0xDD };
        var info = new MiniChunkMetadata(0x0A, 1);
        var chunk = new MiniChunk(info, data);

        var bytes = chunk.Bytes;
        Assert.Equal(3, bytes.Length);

        Assert.Equal(0x0A, bytes[0]); // Type
        Assert.Equal(0x01, bytes[1]); // Size
        Assert.Equal(0xDD, bytes[2]); // Data
    }

    [Fact]
    public void IsChunk_ButNotRootChunk()
    {
        var data = new byte[] { 1 };
        var info = new MiniChunkMetadata(1, 1);
        var chunk = new MiniChunk(info, data);
        Assert.IsType<Chunk>(chunk, false);
        Assert.IsNotType<RootChunk>(chunk, exactMatch: false);
    }
}
