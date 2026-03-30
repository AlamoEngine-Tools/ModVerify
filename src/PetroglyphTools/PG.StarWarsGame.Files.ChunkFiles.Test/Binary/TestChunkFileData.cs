using System;
using PG.StarWarsGame.Files.ChunkFiles.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary;

/// <summary>
/// Provides a reusable, complex test chunk file built using <see cref="ChunkFactory"/>.
/// The file structure is:
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
/// </code>
/// </summary>
public static class TestChunkFileData
{
    /// <summary>
    /// Gets a complex chunk file built using structured chunk types (NodeChunk, MiniNodeChunk, and DataChunk leaves).
    /// </summary>
    public static ChunkFile StructuredFile { get; } = BuildStructuredFile();

    /// <summary>
    /// Gets a chunk file built entirely from RawChunk instances that produce the same binary as <see cref="StructuredFile"/>.
    /// </summary>
    public static ChunkFile RawFile { get; } = BuildRawFile();

    /// <summary>
    /// Gets the expected binary representation of the test chunk file.
    /// </summary>
    public static byte[] ExpectedBytes { get; } = StructuredFile.Bytes;

    private static ChunkFile BuildStructuredFile()
    {
        // Root[0]: leaf data chunk
        var root0 = ChunkFactory.Data(0x01, [0xAA, 0xBB, 0xCC]);

        // Build children for Root[1] NodeChunk
        var child0 = ChunkFactory.Data(0x02, [0x11, 0x22]);

        var grandchild = ChunkFactory.Data(0x03, [0x33]);
        var child1 = ChunkFactory.Node(0x20u, grandchild);

        // MiniNodeChunk as a child - serialize mini-chunks into raw body
        var miniChild0 = ChunkFactory.Mini(0x04, [0x44, 0x55]);
        var miniChild1 = ChunkFactory.Mini(0x05, [0x66]);
        var child2 = ChunkFactory.Node(0x30u, miniChild0, miniChild1);

        var root1 = ChunkFactory.Node(0x10u, child0, child1, child2);

        // Root[2]: leaf data chunk
        var root2 = ChunkFactory.Data(0x06, [0x77, 0x88, 0x99, 0xDD]);

        return ChunkFactory.File(root0, root1, root2);
    }

    private static ChunkFile BuildRawFile()
    {
        // Build the exact same binary using only RawChunk instances.

        // Root[0]
        var root0 = ChunkFactory.Raw(0x01, 3, [0xAA, 0xBB, 0xCC]);

        // child0: RawChunk(type=0x02, size=2, data=[0x11, 0x22]) -> 10 bytes total
        var rawChild0 = ChunkFactory.Raw(0x02, 2, [0x11, 0x22]);

        // grandchild: RawChunk(type=0x03, size=1, data=[0x33]) -> 9 bytes total
        // child1: NodeChunk(type=0x20) wrapping grandchild -> body = grandchild.Bytes (9 bytes)
        var grandchildBytes = ChunkFactory.Raw(0x03, 1, [0x33]).Bytes;
        var rawChild1 = ChunkFactory.Raw(0x20, 0x8000_0000u | (uint)grandchildBytes.Length, grandchildBytes);

        // child2: MiniNodeChunk(type=0x30) with 2 mini-chunks
        // Mini(0x04,[0x44,0x55]) = 4 bytes, Mini(0x05,[0x66]) = 3 bytes -> body = 7 bytes
        var mc0 = ChunkFactory.Mini(0x04, [0x44, 0x55]);
        var mc1 = ChunkFactory.Mini(0x05, [0x66]);
        var miniBody = new byte[mc0.Size + mc1.Size];
        mc0.GetBytes(miniBody);
        mc1.GetBytes(((Span<byte>)miniBody).Slice(mc0.Size));
        var rawChild2 = ChunkFactory.Raw(0x30, (uint)miniBody.Length, miniBody);

        // Root[1]: NodeChunk(type=0x10) body = rawChild0 + rawChild1 + rawChild2
        var root1BodySize = rawChild0.Size + rawChild1.Size + rawChild2.Size;
        var root1Body = new byte[root1BodySize];
        var offset = 0;
        rawChild0.GetBytes(root1Body);
        offset += rawChild0.Size;
        rawChild1.GetBytes(((Span<byte>)root1Body).Slice(offset));
        offset += rawChild1.Size;
        rawChild2.GetBytes(((Span<byte>)root1Body).Slice(offset));
        var root1 = ChunkFactory.Raw(0x10, 0x8000_0000u | (uint)root1BodySize, root1Body);

        // Root[2]
        var root2 = ChunkFactory.Raw(0x06, 4, [0x77, 0x88, 0x99, 0xDD]);

        return ChunkFactory.File(root0, root1, root2);
    }
}
