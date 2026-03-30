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
        var child = CreateChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, new RootChunk[] { child });

        Assert.Equal(info, chunk.Info);
        Assert.Single(chunk.Children);
        Assert.Same(child, chunk.Children[0]);
    }

    [Fact]
    public void Ctor_ThrowsWhenBit31NotSet()
    {
        var child = CreateChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x20, (uint)child.Size);
        Assert.Throws<ArgumentException>(() => new NodeChunk(info, new RootChunk[] { child }));
    }

    [Fact]
    public void Ctor_ThrowsOnNullChildren()
    {
        var info = new ChunkMetadata(0x20, 0x8000_0000u);
        Assert.Throws<ArgumentNullException>(() => new NodeChunk(info, null!));
    }

    [Fact]
    public void Ctor_ThrowsOnEmptyChildren()
    {
        var info = new ChunkMetadata(0x20, 0x8000_0000u);
        Assert.Throws<ArgumentException>(() => new NodeChunk(info, Array.Empty<RootChunk>()));
    }

    [Fact]
    public void Ctor_ThrowsWhenSizeMismatch()
    {
        var child = CreateChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x20, 0x8000_0000u | 999u);
        Assert.Throws<ArgumentException>(() => new NodeChunk(info, new RootChunk[] { child }));
    }

    [Fact]
    public void Size_IncludesHeaderAndChildren()
    {
        var child = CreateChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, new RootChunk[] { child });

        Assert.Equal(8 + child.Size, chunk.Size);
    }

    [Fact]
    public void GetBytes_WritesHeaderAndChildBytes()
    {
        var child = CreateChild(1, new byte[] { 0xFF });
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, new RootChunk[] { child });

        var bytes = chunk.Bytes;
        Assert.Equal(chunk.Size, bytes.Length);

        var childBytes = child.Bytes;
        for (var i = 0; i < childBytes.Length; i++)
            Assert.Equal(childBytes[i], bytes[8 + i]);
    }

    [Fact]
    public void MultipleChildren()
    {
        var c1 = CreateChild(1, new byte[] { 0xAA });
        var c2 = CreateChild(2, new byte[] { 0xBB, 0xCC });
        var totalChildSize = c1.Size + c2.Size;
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)totalChildSize);
        var chunk = new NodeChunk(info, new RootChunk[] { c1, c2 });

        Assert.Equal(2, chunk.Children.Count);
        Assert.Equal(8 + totalChildSize, chunk.Size);
    }

    [Fact]
    public void IsRootChunk()
    {
        var child = CreateChild(1, new byte[] { 0xAA });
        var info = new ChunkMetadata(0x20, 0x8000_0000u | (uint)child.Size);
        var chunk = new NodeChunk(info, new RootChunk[] { child });
        Assert.IsAssignableFrom<RootChunk>(chunk);
    }
}
