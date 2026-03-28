using System;
using System.Diagnostics;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

[DebuggerDisplay("Type: {Type}, Size: {BodySize}, Mini:{IsMiniChunk}")]
public readonly struct ChunkMetadata
{
    public readonly uint Type;
    public readonly uint RawSize;
    public readonly bool IsMiniChunk;

    /// <summary>
    /// Indicates that bit 31 of RawSize is set.
    /// This is a hint that the body contains child chunks, not a guarantee.
    /// </summary>
    public bool HasChildrenHint => !IsMiniChunk && (int)RawSize < 0;

    /// <summary>
    /// Gets the size of the chunk's data in bytes.
    /// </summary>
    /// <remarks>
    /// This value has bit 31 masked off compared to <see cref="RawSize"/>.
    /// Per spec, bit 31 is set only for chunks containing regular child chunks.
    /// Chunks containing mini-chunks (treated as data) do NOT set bit 31.
    /// Since this library doesn't support sizes > <see cref="int.MaxValue"/>, masking bit 31
    /// has no practical impact on the usable size range.
    /// </remarks>
    public int BodySize => (int)(RawSize & 0x7FFF_FFFF);

    public ChunkMetadata(uint type, uint rawSize, bool isMiniChunk)
    {
        if (isMiniChunk && rawSize > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(rawSize), "Mini chunk size must fit in a byte (0-255).");
        Type = type;
        RawSize = rawSize;
        IsMiniChunk = isMiniChunk;
    }
}