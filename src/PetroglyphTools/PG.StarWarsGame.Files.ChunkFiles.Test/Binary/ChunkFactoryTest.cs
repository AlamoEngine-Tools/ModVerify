using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary;

public class ChunkFactoryTest
{
    #region Data

    [Fact]
    public void Data_CreatesDataChunk()
    {
        var chunk = ChunkFactory.Data(0x01, [0xAA, 0xBB]);
        Assert.IsType<DataChunk>(chunk);
        Assert.Equal(0x01u, chunk.Info.Type);
        Assert.Equal(2, chunk.Info.BodySize);
        Assert.False(chunk.Info.HasChildrenHint);
        Assert.Equal(new byte[] { 0xAA, 0xBB }, chunk.Data.ToArray());
    }

    [Fact]
    public void Data_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.Data(1, null!));
    }

    [Fact]
    public void Data_ThrowsOnEmptyArray()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Data(1, []));
    }

    [Fact]
    public void Data_SingleByte()
    {
        var chunk = ChunkFactory.Data(0xFF, [0x42]);
        Assert.Equal(1, chunk.Info.BodySize);
        Assert.Equal(new byte[] { 0x42 }, chunk.Data.ToArray());
    }

    #endregion

    #region Raw

    [Fact]
    public void Raw_CreatesRawChunk()
    {
        var chunk = ChunkFactory.Raw(0x02, 0x8000_0003u, [1, 2, 3]);
        Assert.IsType<RawChunk>(chunk);
        Assert.Equal(0x02u, chunk.Info.Type);
        Assert.Equal(0x8000_0003u, chunk.Info.RawSize);
        Assert.Equal(new byte[] { 1, 2, 3 }, chunk.Data.ToArray());
    }

    [Fact]
    public void Raw_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.Raw(1, 0, null!));
    }

    [Fact]
    public void Raw_ThrowsOnEmptyData()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Raw(0x01, 0, []));
    }

    [Fact]
    public void Raw_ThrowsOnSizeMismatch()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Raw(1, 5, [0xAA]));
    }

    [Fact]
    public void Raw_WithBit31Set_PreservesBit31()
    {
        var chunk = ChunkFactory.Raw(1, 0x8000_0002u, [0xAA, 0xBB]);
        Assert.True(chunk.Info.HasChildrenHint);
        Assert.Equal(2, chunk.Info.BodySize);
    }

    [Fact]
    public void Raw_WithBit31Clear_NoBit31()
    {
        var chunk = ChunkFactory.Raw(1, 2, [0xAA, 0xBB]);
        Assert.False(chunk.Info.HasChildrenHint);
    }

    #endregion

    #region Mini

    [Fact]
    public void Mini_CreatesMiniChunk()
    {
        var chunk = ChunkFactory.Mini(0x05, [0xCC]);
        Assert.IsType<MiniChunk>(chunk);
        Assert.Equal(0x05, chunk.Info.Type);
        Assert.Equal(1, chunk.Info.BodySize);
        Assert.Equal(new byte[] { 0xCC }, chunk.Data.ToArray());
    }

    [Fact]
    public void Mini_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.Mini(1, null!));
    }

    [Fact]
    public void Mini_ThrowsWhenDataExceeds255()
    {
        var data = new byte[256];
        Assert.Throws<ArgumentException>(() => ChunkFactory.Mini(1, data));
    }

    [Fact]
    public void Mini_AllowsMaxLength255()
    {
        var data = new byte[255];
        var chunk = ChunkFactory.Mini(1, data);
        Assert.Equal(255, chunk.Info.BodySize);
    }

    [Fact]
    public void Mini_ThrowsOnEmptyArray()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Mini(1, []));
    }

    [Fact]
    public void Mini_SingleByte()
    {
        var chunk = ChunkFactory.Mini(0, [0x01]);
        Assert.Equal(1, chunk.Info.BodySize);
    }

    #endregion

    #region Node (RootChunk)

    [Fact]
    public void Node_RootChunk_CreatesNodeChunk()
    {
        var child = ChunkFactory.Raw(1, 2, [0xAA, 0xBB]);
        var node = ChunkFactory.Node(0x10, child);

        Assert.IsType<NodeChunk>(node);
        Assert.Equal(0x10u, node.Info.Type);
        Assert.True(node.Info.HasChildrenHint);
        Assert.Single(node.Children);
    }

    [Fact]
    public void Node_RootChunk_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.Node(1, (RootChunk[])null!));
    }

    [Fact]
    public void Node_RootChunk_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Node(1, Array.Empty<RootChunk>()));
    }

    [Fact]
    public void Node_RootChunk_MultipleChildren()
    {
        var c1 = ChunkFactory.Raw(1, 1, [0xAA]);
        var c2 = ChunkFactory.Raw(2, 2, [0xBB, 0xCC]);
        var node = ChunkFactory.Node(0x10, c1, c2);

        Assert.Equal(2, node.Children.Count);
        Assert.True(node.Info.HasChildrenHint);
        Assert.Equal(c1.Size + c2.Size, node.Info.BodySize);
    }

    [Fact]
    public void Node_RootChunk_NestedNodes()
    {
        var leaf = ChunkFactory.Raw(1, 1, [0xAA]);
        var inner = ChunkFactory.Node(2u, leaf);
        var outer = ChunkFactory.Node(3u, inner);

        Assert.Single(outer.Children);
        Assert.IsType<NodeChunk>(outer.Children[0]);
    }

    [Fact]
    public void Node_RootChunk_SetsCorrectMetadataSize()
    {
        var child = ChunkFactory.Raw(1, 2, [0xAA, 0xBB]);
        var node = ChunkFactory.Node(0x10, child);

        // BodySize should equal child's total Size (header + data)
        Assert.Equal(child.Size, node.Info.BodySize);
    }

    #endregion

    #region Node (MiniChunk)

    [Fact]
    public void Node_MiniChunk_CreatesMiniNodeChunk()
    {
        var child = ChunkFactory.Mini(1, [0xAA]);
        var node = ChunkFactory.Node(0x20, child);

        Assert.IsType<MiniNodeChunk>(node);
        Assert.Equal(0x20u, node.Info.Type);
        Assert.False(node.Info.HasChildrenHint);
        Assert.Single(node.Children);
    }

    [Fact]
    public void Node_MiniChunk_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.Node(1, (MiniChunk[])null!));
    }

    [Fact]
    public void Node_MiniChunk_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.Node(1, Array.Empty<MiniChunk>()));
    }

    [Fact]
    public void Node_MiniChunk_MultipleChildren()
    {
        var c1 = ChunkFactory.Mini(1, [0xAA]);
        var c2 = ChunkFactory.Mini(2, [0xBB]);
        var node = ChunkFactory.Node(0x20, c1, c2);

        Assert.Equal(2, node.Children.Count);
        Assert.False(node.Info.HasChildrenHint);
    }

    [Fact]
    public void Node_MiniChunk_SetsCorrectMetadataSize()
    {
        var child = ChunkFactory.Mini(1, [0xAA]);
        var node = ChunkFactory.Node(0x20, child);

        Assert.Equal(child.Size, node.Info.BodySize);
    }

    #endregion

    #region File

    [Fact]
    public void File_CreatesChunkFile()
    {
        var root = ChunkFactory.Raw(1, 1, [0xAA]);
        var file = ChunkFactory.File(root);

        Assert.IsType<ChunkFile>(file);
        Assert.Single(file.RootChunks);
    }

    [Fact]
    public void File_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkFactory.File(null!));
    }

    [Fact]
    public void File_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => ChunkFactory.File());
    }

    [Fact]
    public void File_MultipleRoots()
    {
        var r1 = ChunkFactory.Raw(1, 1, [0xAA]);
        var r2 = ChunkFactory.Raw(2, 2, [0xBB, 0xCC]);
        var file = ChunkFactory.File(r1, r2);

        Assert.Equal(2, file.RootChunks.Count);
    }

    #endregion
}
