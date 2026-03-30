using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary;

/// <summary>
/// Integration test that reads the binary representation of <see cref="TestChunkFileData.ExpectedBytes"/>
/// using <see cref="ChunkReader"/> and reconstructs the original chunk tree structure,
/// asserting that every chunk type, size, and data value matches the expected layout.
/// </summary>
public class ChunkFileIntegrationTest
{
    /// <summary>
    /// Reads the full test chunk file from <see cref="TestChunkFileData.ExpectedBytes"/> and
    /// verifies that the reconstructed chunk tree matches the documented structure exactly.
    /// <code>
    /// Root[0]: DataChunk(type=0x01, data=[0xAA, 0xBB, 0xCC])
    /// Root[1]: NodeChunk(type=0x10)
    ///   Child[0]: DataChunk(type=0x02, data=[0x11, 0x22])
    ///   Child[1]: NodeChunk(type=0x20)
    ///     Child[0]: DataChunk(type=0x03, data=[0x33])
    ///   Child[2]: MiniNodeChunk(type=0x30)
    ///     Mini[0]: MiniChunk(type=0x04, data=[0x44, 0x55])
    ///     Mini[1]: MiniChunk(type=0x05, data=[0x66])
    /// Root[2]: DataChunk(type=0x06, data=[0x77, 0x88, 0x99, 0xDD])
    /// Root[3]: DataChunk(type=0x07, data=[])
    /// Root[4]: NodeChunk(type=0x11, children=[])
    /// Root[5]: MiniNodeChunk(type=0x31)
    ///   Mini[0]: MiniChunk(type=0x08, data=[])
    /// Root[6]: MiniNodeChunk(type=0x32, children=[])
    /// </code>
    /// </summary>
    [Fact]
    public void Read_ReconstructsFullChunkTree()
    {
        var bytes = TestChunkFileData.ExpectedBytes;
        using var stream = new MemoryStream(bytes.ToArray());
        using var reader = new ChunkReader(stream, leaveOpen: true);

        // Root[0]: DataChunk(type=0x01, data=[0xAA, 0xBB, 0xCC])
        var root0 = reader.ReadChunk();
        Assert.Equal(0x01u, root0.Type);
        Assert.False(root0.HasChildrenHint);
        Assert.Equal(3, root0.BodySize);
        Assert.Equal([0xAA, 0xBB, 0xCC], reader.ReadData(root0));

        // Root[1]: NodeChunk(type=0x10) with 3 children
        var root1 = reader.ReadChunk();
        Assert.Equal(0x10u, root1.Type);
        Assert.True(root1.HasChildrenHint);
        var root1Read = 0;

        // Root[1] Child[0]: DataChunk(type=0x02, data=[0x11, 0x22])
        var r1c0 = reader.ReadChunk(ref root1Read);
        Assert.Equal(0x02u, r1c0.Type);
        Assert.False(r1c0.HasChildrenHint);
        Assert.Equal([0x11, 0x22], reader.ReadData(r1c0));
        root1Read += r1c0.BodySize;

        // Root[1] Child[1]: NodeChunk(type=0x20) with 1 child
        var r1c1 = reader.ReadChunk(ref root1Read);
        Assert.Equal(0x20u, r1c1.Type);
        Assert.True(r1c1.HasChildrenHint);
        var r1c1Read = 0;

        // Root[1] Child[1] Child[0]: DataChunk(type=0x03, data=[0x33])
        var r1c1c0 = reader.ReadChunk(ref r1c1Read);
        Assert.Equal(0x03u, r1c1c0.Type);
        Assert.False(r1c1c0.HasChildrenHint);
        Assert.Equal([0x33], reader.ReadData(r1c1c0));
        r1c1Read += r1c1c0.BodySize;
        Assert.Equal(r1c1.BodySize, r1c1Read);
        root1Read += r1c1.BodySize;

        // Root[1] Child[2]: MiniNodeChunk(type=0x30) with 2 mini-chunks
        var r1c2 = reader.ReadChunk(ref root1Read);
        Assert.Equal(0x30u, r1c2.Type);
        Assert.False(r1c2.HasChildrenHint);
        var r1c2Read = 0;

        // Root[1] Child[2] Mini[0]: MiniChunk(type=0x04, data=[0x44, 0x55])
        var r1c2m0 = reader.ReadMiniChunk(ref r1c2Read);
        Assert.Equal(0x04, r1c2m0.Type);
        Assert.Equal([0x44, 0x55], reader.ReadData(r1c2m0));
        r1c2Read += r1c2m0.BodySize;

        // Root[1] Child[2] Mini[1]: MiniChunk(type=0x05, data=[0x66])
        var r1c2m1 = reader.ReadMiniChunk(ref r1c2Read);
        Assert.Equal(0x05, r1c2m1.Type);
        Assert.Equal([0x66], reader.ReadData(r1c2m1));
        r1c2Read += r1c2m1.BodySize;

        Assert.Equal(r1c2.BodySize, r1c2Read);
        root1Read += r1c2.BodySize;
        Assert.Equal(root1.BodySize, root1Read);

        // Root[2]: DataChunk(type=0x06, data=[0x77, 0x88, 0x99, 0xDD])
        var root2 = reader.ReadChunk();
        Assert.Equal(0x06u, root2.Type);
        Assert.False(root2.HasChildrenHint);
        Assert.Equal([0x77, 0x88, 0x99, 0xDD], reader.ReadData(root2));

        // Root[3]: DataChunk(type=0x07, data=[])
        var root3 = reader.ReadChunk();
        Assert.Equal(0x07u, root3.Type);
        Assert.False(root3.HasChildrenHint);
        Assert.Equal(0, root3.BodySize);
        Assert.Equal([], reader.ReadData(root3));

        // Root[4]: NodeChunk(type=0x11, children=[])
        var root4 = reader.ReadChunk();
        Assert.Equal(0x11u, root4.Type);
        Assert.True(root4.HasChildrenHint);
        Assert.Equal(0, root4.BodySize);

        // Root[5]: MiniNodeChunk(type=0x31) with 1 empty mini-chunk
        var root5 = reader.ReadChunk();
        Assert.Equal(0x31u, root5.Type);
        Assert.False(root5.HasChildrenHint);
        var root5Read = 0;

        // Root[5] Mini[0]: MiniChunk(type=0x08, data=[])
        var r5m0 = reader.ReadMiniChunk(ref root5Read);
        Assert.Equal(0x08, r5m0.Type);
        Assert.Equal(0, r5m0.BodySize);
        Assert.Equal([], reader.ReadData(r5m0));
        root5Read += r5m0.BodySize;
        Assert.Equal(root5.BodySize, root5Read);

        // Root[6]: MiniNodeChunk(type=0x32, children=[])
        var root6 = reader.ReadChunk();
        Assert.Equal(0x32u, root6.Type);
        Assert.False(root6.HasChildrenHint);
        Assert.Equal(0, root6.BodySize);

        // Verify entire stream was consumed
        Assert.Equal(stream.Length, stream.Position);
    }
}
