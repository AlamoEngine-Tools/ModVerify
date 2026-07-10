using System;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;


/// <summary>
/// Base class for all chunk types in a chunked file.
/// </summary>
public abstract class Chunk : IBinary
{
    /// <summary>
    /// Gets the total size of this chunk in bytes, including the header.
    /// </summary>
    public abstract int Size { get; }

    /// <inheritdoc />
    public byte[] Bytes
    {
        get
        {
            var bytes = new byte[Size];
            GetBytes(bytes);
            return bytes;
        }
    }

    /// <inheritdoc />
    public abstract void GetBytes(Span<byte> bytes);
}