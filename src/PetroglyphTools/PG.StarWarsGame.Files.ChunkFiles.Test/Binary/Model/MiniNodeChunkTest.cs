using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class MiniNodeChunkTest
{
    private static MiniChunk CreateMiniChild(byte type, byte[] data)
    {
        return new MiniChunk(new MiniChunkMetadata(type, (byte)data.Length), data);
    }

    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var child = CreateMiniChild(1, [0xAA]);
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, [child]);

        Assert.Equal(info, chunk.Info);
        Assert.Single(chunk.Children);
        Assert.Same(child, chunk.Children[0]);
    }

    [Fact]
    public void Ctor_ThrowsOnNullChildren()
    {
        var info = new ChunkMetadata(0x30, 0);
        Assert.Throws<ArgumentNullException>(() => new MiniNodeChunk(info, null!));
    }

    [Fact]
    public void Ctor_AllowsEmptyChildren()
    {
        var info = new ChunkMetadata(0x30, 0);
        var chunk = new MiniNodeChunk(info, []);
        Assert.Empty(chunk.Children);
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var child = CreateMiniChild(1, [0xAA]);
        var info = new ChunkMetadata(0x30, 999);
        Assert.Throws<ArgumentException>(() => new MiniNodeChunk(info, [child]));
    }

    [Fact]
    public void Ctor_ThrowsWhenRawSizeExceedsIntMax()
    {
        var child = CreateMiniChild(1, [0xAA]);
        // RawSize with bit 31 set exceeds int.MaxValue
        var info = new ChunkMetadata(0x30, 0x8000_0000u | (uint)child.Size);
        Assert.Throws<NotSupportedException>(() => new MiniNodeChunk(info, [child]));
    }

    [Fact]
    public void Size_IncludesHeaderAndChildren()
    {
        var child = CreateMiniChild(1, [0xAA]);
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, [child]);

        Assert.Equal(8 + child.Size, chunk.Size);
    }

    [Fact]
    public void GetBytes_WritesExactByteSequence()
    {
        // child: type=0x01, data=[0xAA] => MiniChunk header [01 01] + [AA] = 3 bytes
        var child = CreateMiniChild(1, [0xAA]);
        // MiniNodeChunk header: type=0x30, RawSize=3
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, [child]);

        var bytes = chunk.Bytes;

        byte[] expected =
        [
            // MiniNodeChunk header (8 bytes): type=0x00000030, RawSize=0x00000003
            0x30, 0x00, 0x00, 0x00,
            0x03, 0x00, 0x00, 0x00,
            // child header: type=0x01, size=0x01
            0x01, 0x01,
            // child data
            0xAA
        ];
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void GetBytes_EmptyChildren_WritesHeaderOnly()
    {
        var info = new ChunkMetadata(0x30, 0);
        var chunk = new MiniNodeChunk(info, []);

        var bytes = chunk.Bytes;

        byte[] expected =
        [
            // MiniNodeChunk header: type=0x00000030, RawSize=0x00000000
            0x30, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ];
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void IsRootChunk()
    {
        var child = CreateMiniChild(1, [0xAA]);
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, [child]);
        Assert.IsType<RootChunk>(chunk, false);
    }

    [Fact]
    public void MultipleChildren()
    {
        var c1 = CreateMiniChild(1, [0xAA]);
        var c2 = CreateMiniChild(2, [0xBB, 0xCC]);
        var totalChildSize = c1.Size + c2.Size;
        var info = new ChunkMetadata(0x30, (uint)totalChildSize);
        var chunk = new MiniNodeChunk(info, [c1, c2]);

        Assert.Equal(2, chunk.Children.Count);
        Assert.Equal(8 + totalChildSize, chunk.Size);
    }
}
