using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

public sealed class Chunk : IBinary
{
    public ChunkMetadata Info { get; }

    public byte[]? Data { get; }

    public IReadOnlyList<Chunk> Children { get; }

    public byte[] Bytes
    {
        get
        {
            var bytes = new byte[Size];
            GetBytes(bytes);
            return bytes;
        }
    }

    public int Size => Info.IsMiniChunk
        ? 2 + Data!.Length
        : Data is not null
            ? 8 + Data.Length
            : 8 + Children.Sum(c => c.Size);

    public Chunk(ChunkMetadata info, byte[] data)
    {
        Info = info;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Children = [];
    }

    public Chunk(ChunkMetadata info, IReadOnlyList<Chunk> children)
    {
        if (info.IsMiniChunk)
            throw new ArgumentException("MiniChunks cannot have child chunks", nameof(info));
        Info = info;
        Data = null;
        Children = children ?? throw new ArgumentNullException(nameof(children));
    }

    public void GetBytes(Span<byte> bytes)
    {
        if (Info.IsMiniChunk)
        {
            bytes[0] = (byte)Info.Type;
            bytes[1] = (byte)Data!.Length;
            Data.AsSpan().CopyTo(bytes[2..]);
            return;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Info.Type);

        if (Data is not null)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], Data.Length);
            Data.AsSpan().CopyTo(bytes[8..]);
        }
        else
        {
            // .Sum is a checked operation and will already throw an overflow exception
            var bodySize = Children.Sum(c => c.Size);
            var hasMiniChunkChildren = Children.Count > 0 && Children[0].Info.IsMiniChunk;
            
            var sizeField = hasMiniChunkChildren
                ? bodySize
                : (int)(bodySize | 0x8000_0000u);

            BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], sizeField);

            var offset = 8;
            foreach (var child in Children)
            {
                child.GetBytes(bytes[offset..]);
                offset += child.Size;
            }
        }
    }
}

/// <summary>
/// Provides factory methods for creating chunks and chunk files.
/// </summary>
/// <remarks>
/// <para>
/// This class provides static methods for creating hierarchical chunk structures.
/// Three chunk types are supported:
/// </para>
/// <list type="bullet">
/// <item><description>Data chunks - Regular chunks containing binary data</description></item>
/// <item><description>Mini-chunks - Chunks with 2-byte headers for data up to 255 bytes</description></item>
/// <item><description>Node chunks - Container chunks that hold child chunks</description></item>
/// </list>
/// </remarks>
public static class ChunkFactory
{
    /// <summary>
    /// Creates a data chunk with the specified type and binary data.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="data">The binary data to store in the chunk.</param>
    /// <returns>A <see cref="Chunk"/> containing the specified data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static Chunk Data(uint type, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var metadata = new ChunkMetadata(type, (uint)data.Length, false);
        return new Chunk(metadata, data);
    }

    /// <summary>
    /// Creates a mini-chunk with the specified type and binary data.
    /// </summary>
    /// <param name="type">The mini-chunk type identifier.</param>
    /// <param name="data">The binary data to store in the mini-chunk. Maximum length is 255 bytes.</param>
    /// <returns>A <see cref="Chunk"/> representing a mini-chunk with a 2-byte header.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The length of <paramref name="data"/> exceeds 255 bytes.</exception>
    public static Chunk Mini(byte type, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (data.Length > byte.MaxValue)
            throw new ArgumentException(
                $"Mini-chunk data cannot exceed {byte.MaxValue} bytes. Provided data length: {data.Length}.",
                nameof(data));

        var metadata = new ChunkMetadata(type, (uint)data.Length, true);
        return new Chunk(metadata, data);
    }

    /// <summary>
    /// Creates a chunk node that contains child chunks.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="children">The child chunks to include in this node.</param>
    /// <returns>A <see cref="Chunk"/> containing the specified children.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> is <see langword="null"/>.</exception>
    public static Chunk Node(uint type, params Chunk[] children)
    {
        if (children == null)
            throw new ArgumentNullException(nameof(children));

        var size = (uint)children.Sum(c => c.Size);
        var metadata = new ChunkMetadata(type, size, false);
        return new Chunk(metadata, children);
    }

    /// <summary>
    /// Creates a chunk node that contains child chunks built using a configuration action.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="configure">An action that populates a list with child chunks.</param>
    /// <returns>A <see cref="Chunk"/> containing the configured children.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public static Chunk Node(uint type, Action<List<Chunk>> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var children = new List<Chunk>();
        configure(children);
        return Node(type, children.ToArray());
    }

    /// <summary>
    /// Creates a chunk file containing the specified root chunks.
    /// </summary>
    /// <param name="rootChunks">The top-level chunks to include in the file.</param>
    /// <returns>A <see cref="ChunkFile"/> containing the specified root chunks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rootChunks"/> is <see langword="null"/>.</exception>
    public static ChunkFile File(params Chunk[] rootChunks)
    {
        if (rootChunks == null)
            throw new ArgumentNullException(nameof(rootChunks));

        return new ChunkFile(rootChunks);
    }
}