using System.Diagnostics;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;

/// <summary>
/// Describes the header of a mini-chunk.
/// </summary>
/// <remarks>
/// A mini-chunk header is 2 bytes: 1 byte for the type and 1 byte for the size.
/// Mini-chunks cannot contain children.
/// </remarks>
[DebuggerDisplay("Type: 0x{Type:X2}, Size: {BodySize}")]
public readonly struct MiniChunkMetadata
{
    /// <summary>
    /// The mini-chunk type identifier.
    /// </summary>
    public readonly byte Type;

    /// <summary>
    /// The size of the mini-chunk body in bytes.
    /// </summary>
    public readonly byte BodySize;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiniChunkMetadata"/> struct.
    /// </summary>
    /// <param name="type">The mini-chunk type identifier.</param>
    /// <param name="size">The size of the mini-chunk body in bytes.</param>
    public MiniChunkMetadata(byte type, byte size)
    {
        Type = type;
        BodySize = size;
    }
}