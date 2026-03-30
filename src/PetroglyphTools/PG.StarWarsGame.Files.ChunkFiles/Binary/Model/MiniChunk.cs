using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// A mini-chunk containing binary data with a compact 2-byte header.
/// </summary>
/// <remarks>
/// <para>
/// Mini-chunks use a 2-byte header (1 byte type, 1 byte size) instead of the standard 8-byte header.
/// They can only appear as children of a <see cref="MiniNodeChunk"/>.
/// </para>
/// <para>
/// A mini chunk cannot be used as a root chunk or as a child of a <see cref="NodeChunk"/>.
/// </para>
/// </remarks>
public sealed class MiniChunk : Chunk
{
    /// <summary>
    /// Gets the mini-chunk metadata.
    /// </summary>
    public MiniChunkMetadata Info { get; }

    /// <summary>
    /// Gets the mini-chunk's binary data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <inheritdoc />
    public override unsafe int Size => sizeof(MiniChunkMetadata) + Data.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiniChunk"/> class.
    /// </summary>
    /// <param name="info">The mini-chunk metadata.</param>
    /// <param name="data">The binary data payload. Maximum length is 255 bytes.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="data"/> is empty, or
    /// <paramref name="info"/> size does not match the data length.
    /// </exception>
    public MiniChunk(MiniChunkMetadata info, ReadOnlyMemory<byte> data)
    {
        if (data is { IsEmpty: true, Length: 0 })
            throw new ArgumentException("Data cannot be empty.", nameof(data));

        if (info.BodySize!= data.Length)
            throw new ArgumentException(
                $"Metadata size ({info.BodySize}) does not match data length ({data.Length}).",
                nameof(info));

        Info = info;
        Data = data;
    }

    /// <inheritdoc />
    public override unsafe void GetBytes(Span<byte> bytes)
    {
        bytes[0] = Info.Type;
        bytes[1] = Info.BodySize;
        Data.Span.CopyTo(bytes[sizeof(MiniChunkMetadata)..]);
    }
}