using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Buffers.Binary;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// A chunk that stores raw binary data without interpreting its contents.
/// </summary>
/// <remarks>
/// <para>
/// This chunk stores its body as a raw byte blob. The body may contain
/// any content: raw data, child chunks, mini-chunks, or any combination.
/// No structural interpretation or validation is performed on the body.
/// </para>
/// <para>
/// This is useful for:
/// </para>
/// <list type="bullet">
///   <item><description>Round-tripping unknown or unparsed chunks.</description></item>
///   <item><description>Creating chunks from pre-serialized data.</description></item>
/// </list>
/// <para>
/// The metadata is written as-is, including bit 31 of the size field.
/// </para>
/// </remarks>
public sealed class RawChunk : RootChunk
{
    /// <summary>
    /// Gets the chunk metadata.
    /// </summary>
    public ChunkMetadata Info { get; }

    /// <summary>
    /// Gets the raw body data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <inheritdoc />
    public override unsafe int Size => sizeof(ChunkMetadata) + Data.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawChunk"/> class.
    /// </summary>
    /// <param name="info">The chunk metadata, written as-is. No validation is performed on bit 31.</param>
    /// <param name="data">The raw body data.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="data"/> is empty, or
    /// <paramref name="info"/> body size does not match the data length.
    /// </exception>
    public RawChunk(ChunkMetadata info, ReadOnlyMemory<byte> data)
    {
        if (data is { IsEmpty: true, Length: 0 })
            throw new ArgumentException("Data cannot be empty.", nameof(data));

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
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[sizeof(uint)..], Info.RawSize);
        Data.Span.CopyTo(bytes[sizeof(ChunkMetadata)..]);
    }
}