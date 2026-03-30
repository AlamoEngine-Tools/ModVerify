using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PG.StarWarsGame.Files.Binary.File;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// Represents a chunked file containing one or more root-level chunks.
/// </summary>
public sealed class ChunkFile : IBinaryFile
{
    /// <summary>
    /// Gets the root-level chunks in this file.
    /// </summary>
    public IReadOnlyList<RootChunk> RootChunks { get; }

    /// <summary>
    /// Gets the total file size in bytes.
    /// </summary>
    public int Size => RootChunks.Sum(c => c.Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkFile"/> class.
    /// </summary>
    /// <param name="rootChunks">
    /// The root-level chunks. Must contain at least one element.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rootChunks"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="rootChunks"/> is empty.
    /// </exception>
    public ChunkFile(IReadOnlyList<RootChunk> rootChunks)
    {
        if (rootChunks == null)
            throw new ArgumentNullException(nameof(rootChunks));

        if (rootChunks.Count == 0)
            throw new ArgumentException("ChunkFile must have at least one root chunk.", nameof(rootChunks));

        RootChunks = rootChunks;
    }

    /// <summary>
    /// Gets the file's binary representation as a byte array.
    /// </summary>
    public byte[] Bytes
    {
        get
        {
            var bytes = new byte[Size];
            GetBytes(bytes);
            return bytes;
        }
    }

    /// <summary>
    /// Writes the file's binary representation into the specified span.
    /// </summary>
    /// <param name="bytes">
    /// The destination span. Must be at least <see cref="Size"/> bytes long.
    /// </param>
    public void GetBytes(Span<byte> bytes)
    {
        var offset = 0;
        foreach (var chunk in RootChunks)
        {
            chunk.GetBytes(bytes[offset..]);
            offset += chunk.Size;
        }
    }

    /// <summary>
    /// Writes the file's binary representation to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public void WriteTo(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.Write(Bytes, 0, Size);
    }
}