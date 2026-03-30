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
        var child = CreateMiniChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, new MiniChunk[] { child });

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
    public void Ctor_ThrowsOnEmptyChildren()
    {
        var info = new ChunkMetadata(0x30, 0);
        Assert.Throws<ArgumentException>(() => new MiniNodeChunk(info, Array.Empty<MiniChunk>()));
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var child = CreateMiniChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x30, 999);
        Assert.Throws<ArgumentException>(() => new MiniNodeChunk(info, new MiniChunk[] { child }));
    }

    [Fact]
    public void Ctor_ThrowsWhenRawSizeExceedsIntMax()
    {
        var child = CreateMiniChild(1, new byte[] { 0xAA });
        // RawSize with bit 31 set exceeds int.MaxValue
        var info = new ChunkMetadata(0x30, 0x8000_0000u | (uint)child.Size);
        Assert.Throws<NotSupportedException>(() => new MiniNodeChunk(info, new MiniChunk[] { child }));
    }

    [Fact]
    public void Size_IncludesHeaderAndChildren()
    {
        var child = CreateMiniChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, new MiniChunk[] { child });

        Assert.Equal(8 + child.Size, chunk.Size);
    }

    [Fact]
    public void IsRootChunk()
    {
        var child = CreateMiniChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x30, (uint)child.Size);
        var chunk = new MiniNodeChunk(info, new MiniChunk[] { child });
        Assert.IsAssignableFrom<RootChunk>(chunk);
    }

    [Fact]
    public void MultipleChildren()
    {
        var c1 = CreateMiniChild(1, new byte[] { 0xAA });
        var c2 = CreateMiniChild(2, new byte[] { 0xBB, 0xCC });
        var totalChildSize = c1.Size + c2.Size;
        var info = new ChunkMetadata(0x30, (uint)totalChildSize);
        var chunk = new MiniNodeChunk(info, new MiniChunk[] { c1, c2 });

        Assert.Equal(2, chunk.Children.Count);
        Assert.Equal(8 + totalChildSize, chunk.Size);
    }
}
