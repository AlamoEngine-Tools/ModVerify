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
/// Root[3]: DataChunk(type=0x07, data=[])           -- empty data chunk
/// Root[4]: NodeChunk(type=0x11, children=[])        -- empty node chunk
/// Root[5]: MiniNodeChunk(type=0x31)                  -- mini-node chunk with one empty mini-chunk
///   Mini[0]: MiniChunk(type=0x08, data=[])            -- empty mini-chunk
/// Root[6]: MiniNodeChunk(type=0x32, children=[])    -- empty mini-node chunk
/// </code>
/// </summary>
public static class TestChunkFileData
{
    /// <summary>
    /// Gets a complex chunk file built using structured chunk types (NodeChunk, MiniNodeChunk, and DataChunk leaves),
    /// including chunks with empty data, a MiniNodeChunk with one empty MiniChunk, and an empty MiniNodeChunk to exercise those allowed edge cases.
    /// </summary>
    public static ChunkFile StructuredFile { get; } = BuildStructuredFile();

    /// <summary>
    /// Gets the expected binary representation of the test chunk file,
    /// matching the binary output of <see cref="StructuredFile"/>.
    /// </summary>
    public static ReadOnlyMemory<byte> ExpectedBytes { get; } = new byte[]
    {
        // Root[0]: DataChunk(type=0x01, data=[0xAA, 0xBB, 0xCC])
        0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xAA, 0xBB, 0xCC,
        // Root[1]: NodeChunk(type=0x10, bodySize=0x2A | bit31)
        0x10, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x80,
          // Child[0]: DataChunk(type=0x02, data=[0x11, 0x22])
          0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x11, 0x22,
          // Child[1]: NodeChunk(type=0x20, bodySize=0x09 | bit31)
          0x20, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x80,
            // Grandchild: DataChunk(type=0x03, data=[0x33])
            0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x33,
          // Child[2]: MiniNodeChunk(type=0x30, bodySize=7)
          0x30, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00,
            // Mini[0]: MiniChunk(type=0x04, data=[0x44, 0x55])
            0x04, 0x02, 0x44, 0x55,
            // Mini[1]: MiniChunk(type=0x05, data=[0x66])
            0x05, 0x01, 0x66,
        // Root[2]: DataChunk(type=0x06, data=[0x77, 0x88, 0x99, 0xDD])
        0x06, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x77, 0x88, 0x99, 0xDD,
        // Root[3]: DataChunk(type=0x07, data=[])
        0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        // Root[4]: NodeChunk(type=0x11, children=[])
        0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
        // Root[5]: MiniNodeChunk(type=0x31) with Mini[0]: MiniChunk(type=0x08, data=[])
        0x31, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
          0x08, 0x00,
        // Root[6]: MiniNodeChunk(type=0x32, children=[])
        0x32, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    };

    private static ChunkFile BuildStructuredFile()
    {
        return ChunkFactory.File(
            ChunkFactory.Data(0x01, [0xAA, 0xBB, 0xCC]),
            ChunkFactory.Node(0x10u,
                ChunkFactory.Data(0x02, [0x11, 0x22]),
                ChunkFactory.Node(0x20u,
                    ChunkFactory.Data(0x03, [0x33])),
                ChunkFactory.Node(0x30u,
                    ChunkFactory.Mini(0x04, [0x44, 0x55]),
                    ChunkFactory.Mini(0x05, [0x66]))),
            ChunkFactory.Data(0x06, [0x77, 0x88, 0x99, 0xDD]),
            ChunkFactory.Data(0x07, []),
            ChunkFactory.Node(0x11u, (RootChunk[])[]),
            ChunkFactory.Node(0x31u, ChunkFactory.Mini(0x08, [])),
            ChunkFactory.Node(0x32u, (MiniChunk[])[])
        );
    }

}
