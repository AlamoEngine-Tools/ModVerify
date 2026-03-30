using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary;

/// <summary>
/// Systematically verifies that every structured chunk type produces the exact same binary
/// as a <see cref="PG.StarWarsGame.Files.ChunkFiles.Binary.Model.RawChunk"/> constructed
/// from the equivalent header and body bytes.
/// </summary>
public class RawChunkEquivalenceTest
{
    #region DataChunk

    [Fact]
    public void DataChunk_WithData_MatchesRaw()
    {
        var structured = ChunkFactory.Data(0x01, [0xAA, 0xBB, 0xCC]);
        var raw = ChunkFactory.Raw(0x01, 3, [0xAA, 0xBB, 0xCC]);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void DataChunk_SingleByte_MatchesRaw()
    {
        var structured = ChunkFactory.Data(0xFF, [0x42]);
        var raw = ChunkFactory.Raw(0xFF, 1, [0x42]);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void DataChunk_EmptyData_MatchesRaw()
    {
        var structured = ChunkFactory.Data(0x07, []);
        var raw = ChunkFactory.Raw(0x07, 0, []);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    #endregion

    #region MiniNodeChunk

    [Fact]
    public void MiniNodeChunk_WithChildren_MatchesRaw()
    {
        var structured = ChunkFactory.Node(0x30u,
            ChunkFactory.Mini(0x04, [0x44, 0x55]),
            ChunkFactory.Mini(0x05, [0x66]));

        // Body = mini0.Bytes + mini1.Bytes = [0x04,0x02,0x44,0x55] + [0x05,0x01,0x66]
        var body = new byte[] { 0x04, 0x02, 0x44, 0x55, 0x05, 0x01, 0x66 };
        var raw = ChunkFactory.Raw(0x30, (uint)body.Length, body);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void MiniNodeChunk_SingleChild_MatchesRaw()
    {
        var structured = ChunkFactory.Node(0x31u, ChunkFactory.Mini(0x08, []));

        var body = new byte[] { 0x08, 0x00 };
        var raw = ChunkFactory.Raw(0x31, (uint)body.Length, body);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void MiniNodeChunk_EmptyChildren_MatchesRaw()
    {
        var structured = ChunkFactory.Node(0x32u, (MiniChunk[])[]);
        var raw = ChunkFactory.Raw(0x32, 0u, []);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    #endregion

    #region NodeChunk

    [Fact]
    public void NodeChunk_WithDataChildren_MatchesRaw()
    {
        var structured = ChunkFactory.Node(0x20u,
            ChunkFactory.Data(0x03, [0x33]));

        // Body = DataChunk(0x03, [0x33]).Bytes = 8-byte header + [0x33]
        var childBytes = ChunkFactory.Data(0x03, [0x33]).Bytes;
        var raw = ChunkFactory.Raw(0x20, 0x8000_0000u | (uint)childBytes.Length, childBytes);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void NodeChunk_WithMultipleDataChildren_MatchesRaw()
    {
        var child0 = ChunkFactory.Data(0x02, [0x11, 0x22]);
        var child1 = ChunkFactory.Data(0x03, [0x33]);
        var structured = ChunkFactory.Node(0x10u, child0, child1);

        var body = new byte[child0.Size + child1.Size];
        child0.GetBytes(body);
        child1.GetBytes(((Span<byte>)body).Slice(child0.Size));
        var raw = ChunkFactory.Raw(0x10, 0x8000_0000u | (uint)body.Length, body);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void NodeChunk_EmptyChildren_MatchesRaw()
    {
        var structured = ChunkFactory.Node(0x11u, (RootChunk[])[]);
        var raw = ChunkFactory.Raw(0x11, 0x8000_0000u, []);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void NodeChunk_WithNestedNodeChild_MatchesRaw()
    {
        var grandchild = ChunkFactory.Data(0x03, [0x33]);
        var innerNode = ChunkFactory.Node(0x20u, grandchild);
        var structured = ChunkFactory.Node(0x10u, innerNode);

        var innerBytes = innerNode.Bytes;
        var raw = ChunkFactory.Raw(0x10, 0x8000_0000u | (uint)innerBytes.Length, innerBytes);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    [Fact]
    public void NodeChunk_WithMiniNodeChild_MatchesRaw()
    {
        var miniNode = ChunkFactory.Node(0x30u,
            ChunkFactory.Mini(0x04, [0x44, 0x55]),
            ChunkFactory.Mini(0x05, [0x66]));
        var structured = ChunkFactory.Node(0x10u, miniNode);

        var miniNodeBytes = miniNode.Bytes;
        var raw = ChunkFactory.Raw(0x10, 0x8000_0000u | (uint)miniNodeBytes.Length, miniNodeBytes);

        Assert.Equal(structured.Bytes, raw.Bytes);
    }

    #endregion

    #region ChunkFile

    [Fact]
    public void ChunkFile_StructuredFile_MatchesExpectedBytes()
    {
        Assert.Equal(TestChunkFileData.ExpectedBytes.ToArray(), TestChunkFileData.StructuredFile.Bytes);
    }

    [Fact]
    public void ChunkFile_StructuredFile_HasExpectedSize()
    {
        Assert.Equal(TestChunkFileData.ExpectedBytes.Length, TestChunkFileData.StructuredFile.Size);
    }

    [Fact]
    public void ChunkFile_StructuredFile_HasExpectedRootCount()
    {
        Assert.Equal(7, TestChunkFileData.StructuredFile.RootChunks.Count);
    }

    #endregion
}
