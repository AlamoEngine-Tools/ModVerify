using System.Diagnostics;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;

/// <summary>
/// Describes the header of a regular chunk.
/// </summary>
/// <remarks>
/// A regular chunk header is 8 bytes: 4 bytes for the type and 4 bytes for the size.
/// Bit 31 of the size field indicates whether the chunk body contains child chunks.
/// </remarks>
[DebuggerDisplay("Type: 0x{Type:X8}, Size: {BodySize}, HasChildrenHint: {HasChildrenHint}")]
public readonly struct ChunkMetadata
{
    /// <summary>
    /// The chunk type identifier.
    /// </summary>
    public readonly uint Type;

    /// <summary>
    /// The raw size value as stored in the chunk header, including bit 31.
    /// </summary>
    public readonly uint RawSize;

    /// <summary>
    /// Gets a value indicating whether bit 31 of <see cref="RawSize"/> is set.
    /// This is a hint that the body contains child chunks, not a guarantee.
    /// </summary>
    public bool HasChildrenHint => (int)RawSize < 0;

    /// <summary>
    /// Gets the size of the chunk body in bytes with bit 31 masked off.
    /// </summary>
    public int BodySize => (int)(RawSize & 0x7FFF_FFFF);

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkMetadata"/> struct.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="rawSize">
    /// The raw size value. Bit 31 should be set if the chunk contains child chunks.
    /// </param>
    public ChunkMetadata(uint type, uint rawSize)
    {
        Type = type;
        RawSize = rawSize;
    }
}