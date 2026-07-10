using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model;

public class NodeChunkTest
{
    private static RawChunk CreateChild(uint type, byte[] data)
    {
        return new RawChunk(new ChunkMetadata(type, (uint)data.Length), data);
    }

    [Fact]
    public void Ctor_ValidArgs_SetsProperties()
    {
        var child = CreateChild(1, [0xAA]);
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, [child]);

        Assert.Equal(info, chunk.Info);
        Assert.Single(chunk.Children);
        Assert.Same(child, chunk.Children[0]);
    }

    [Fact]
    public void Ctor_ThrowsWhenBit31NotSet()
    {
        var child = CreateChild(1, [0xAA]);
        var info = new ChunkMetadata(0x20, (uint)child.Size);
        Assert.Throws<ArgumentException>(() => new NodeChunk(info, [child]));
    }

    [Fact]
    public void Ctor_ThrowsOnNullChildren()
    {
        var info = new ChunkMetadata(0x20, 0x8000_0000u);
        Assert.Throws<ArgumentNullException>(() => new NodeChunk(info, null!));
    }

    [Fact]
    public void Ctor_AllowsEmptyChildren()
    {
        var info = new ChunkMetadata(0x20, 0x8000_0000u);
        var chunk = new NodeChunk(info, []);
        Assert.Empty(chunk.Children);
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var child = CreateChild(1, [0xAA]);
        var info = new ChunkMetadata(0x20, 0x8000_0000u | 999u);
        Assert.Throws<ArgumentException>(() => new NodeChunk(info, [child]));
    }

    [Fact]
    public void Size_IncludesHeaderAndChildren()
    {
        var child = CreateChild(1, [0xAA]);
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, [child]);

        Assert.Equal(8 + child.Size, chunk.Size);
    }

    [Fact]
    public void GetBytes_WritesExactByteSequence()
    {
        // child: type=0x01, data=[0xFF] => RawChunk header [01 00 00 00  01 00 00 00] + [FF] = 9 bytes
        var child = CreateChild(1, [0xFF]);
        // NodeChunk header: type=0x20, RawSize = 0x8000_0000 | 9 = 0x80000009
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, [child]);

        var bytes = chunk.Bytes;

        byte[] expected =
        [
            // NodeChunk header (8 bytes): type=0x00000020, RawSize=0x80000009
            0x20, 0x00, 0x00, 0x00,
            0x09, 0x00, 0x00, 0x80,
            // child header: type=0x00000001, RawSize=0x00000001
            0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            // child data
            0xFF
        ];
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void GetBytes_EmptyChildren_WritesHeaderOnly()
    {
        var info = new ChunkMetadata(0x20, 0x8000_0000u);
        var chunk = new NodeChunk(info, []);

        var bytes = chunk.Bytes;

        byte[] expected =
        [
            // NodeChunk header: type=0x00000020, RawSize=0x80000000
            0x20, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x80
        ];
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void MultipleChildren()
    {
        var c1 = CreateChild(1, [0xAA]);
        var c2 = CreateChild(2, [0xBB, 0xCC]);
        var totalChildSize = c1.Size + c2.Size;
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)totalChildSize);
        var chunk = new NodeChunk(info, [c1, c2]);

        Assert.Equal(2, chunk.Children.Count);
        Assert.Equal(8 + totalChildSize, chunk.Size);
    }

    [Fact]
    public void IsRootChunk()
    {
        var child = CreateChild(1, [0xAA]);
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, [child]);
        Assert.IsType<RootChunk>(chunk, false);
    }
}
