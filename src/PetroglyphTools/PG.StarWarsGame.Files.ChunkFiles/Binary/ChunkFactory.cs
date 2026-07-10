using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Diagnostics;
using System.Linq;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary;

/// <summary>
/// Provides factory methods for creating chunks and chunk files.
/// </summary>
/// <remarks>
/// <para>Five chunk types are supported:</para>
/// <list type="bullet">
///   <item><description><see cref="DataChunk"/> — Regular chunk containing binary data.</description></item>
///   <item><description><see cref="MiniChunk"/> — Chunk with a smaller, 2-byte header for data up to 255 bytes.</description></item>
///   <item><description><see cref="NodeChunk"/> — Container chunk holding regular child chunks.</description></item>
///   <item><description><see cref="MiniNodeChunk"/> — Container chunk holding mini-chunk children.</description></item>
///   <item><description><see cref="RawChunk"/> — Chunk holding raw binary data.</description></item>
/// </list>
/// </remarks>
public static class ChunkFactory
{
    /// <summary>
    /// Creates a data chunk with the specified type and binary data.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="data">The binary data to store in the chunk.</param>
    /// <returns>A new <see cref="DataChunk"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static DataChunk Data(uint type, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        var metadata = new ChunkMetadata(type, (uint)data.Length);
        return new DataChunk(metadata, data);
    }

    /// <summary>
    /// Creates a raw chunk that stores its body as an uninterpreted byte blob.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="rawSize">The raw size value written to the header. The high bit 31, set or unset, is not interpreted.</param>
    /// <param name="data">The raw body data.</param>
    /// <returns>A new <see cref="RawChunk"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static RawChunk Raw(uint type, uint rawSize, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        var metadata = new ChunkMetadata(type, rawSize);
        return new RawChunk(metadata, data);
    }

    /// <summary>
    /// Creates a mini-chunk with the specified type and binary data.
    /// </summary>
    /// <param name="type">The mini-chunk type identifier.</param>
    /// <param name="data">The binary data to store. Maximum length is 255 bytes.</param>
    /// <returns>A new <see cref="MiniChunk"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The length of <paramref name="data"/> exceeds 255 bytes.</exception>
    public static MiniChunk Mini(byte type, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (data.Length > byte.MaxValue)
            throw new ArgumentException($"Mini-chunk data cannot exceed {byte.MaxValue} bytes. Provided: {data.Length}.", 
                nameof(data));

        var metadata = new MiniChunkMetadata(type, (byte)data.Length);
        return new MiniChunk(metadata, data);
    }

    /// <summary>
    /// Creates a node chunk containing regular child chunks.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="children">The child chunks.</param>
    /// <returns>A new <see cref="NodeChunk"/> with bit 31 set in the size field.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The total size of the child chunks exceeds 2GB.</exception>
    public static NodeChunk Node(uint type, params RootChunk[] children)
    {
        if (children == null)
            throw new ArgumentNullException(nameof(children));

        var size = (uint)children.Sum(c => c.Size);
        if (size > int.MaxValue)
            throw new InvalidOperationException("Chunk nodes cannot contain chunks with a total content size larger than 2GB.");
        var metadata = new ChunkMetadata(type, size | 0x8000_0000u);
        Debug.Assert((int)metadata.RawSize < 0);
        return new NodeChunk(metadata, children);
    }

    /// <summary>
    /// Creates a node chunk containing mini-chunk children.
    /// </summary>
    /// <param name="type">The chunk type identifier.</param>
    /// <param name="children">The mini-chunk children.</param>
    /// <returns>A new <see cref="MiniNodeChunk"/> with bit 31 cleared in the size field.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> is <see langword="null"/>.</exception>
    public static MiniNodeChunk Node(uint type, params MiniChunk[] children)
    {
        if (children == null)
            throw new ArgumentNullException(nameof(children));

        var size = (uint)children.Sum(c => c.Size);
        var metadata = new ChunkMetadata(type, size);
        return new MiniNodeChunk(metadata, children);
    }

    /// <summary>
    /// Creates a chunk file containing the specified root chunks.
    /// </summary>
    /// <param name="rootChunks">The top-level chunks.</param>
    /// <returns>A new <see cref="ChunkFile"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rootChunks"/> is <see langword="null"/>.</exception>
    public static ChunkFile File(params RootChunk[] rootChunks)
    {
        if (rootChunks == null)
            throw new ArgumentNullException(nameof(rootChunks));
        return new ChunkFile(rootChunks);
    }
}