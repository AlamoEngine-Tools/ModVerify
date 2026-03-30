using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Buffers.Binary;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// A chunk containing binary data.
/// </summary>
public sealed class DataChunk : RootChunk
{
    /// <summary>
    /// Gets the chunk metadata.
    /// </summary>
    public ChunkMetadata Info { get; }

    /// <summary>
    /// Gets the chunk's binary data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <inheritdoc />
    public override unsafe int Size => sizeof(ChunkMetadata) + Data.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataChunk"/> class.
    /// </summary>
    /// <param name="info">The chunk metadata. Must not have bit 31 set.</param>
    /// <param name="data">The binary data payload.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="data"/> is empty, or
    /// <paramref name="info"/> has bit 31 set, or
    /// <paramref name="info"/> body size does not match the data length.
    /// </exception>
    public DataChunk(ChunkMetadata info, ReadOnlyMemory<byte> data)
    {
        if (data is { IsEmpty: true, Length: 0 })
            throw new ArgumentException("Data cannot be empty.", nameof(data));

        if (info.HasChildrenHint)
            throw new ArgumentException(
                "DataChunk metadata must not have bit 31 set.", nameof(info));

        if (info.BodySize != data.Length)
            throw new ArgumentException(
                $"Metadata size ({info.BodySize}) does not match data length ({data.Length}).",
                nameof(info));

        Info = info;
        Data = data;
    }

    /// <inheritdoc />
    public override unsafe void GetBytes(Span<byte> bytes)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Info.Type);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[sizeof(uint)..], Data.Length);
        Data.Span.CopyTo(bytes[sizeof(ChunkMetadata)..]);
    }
}