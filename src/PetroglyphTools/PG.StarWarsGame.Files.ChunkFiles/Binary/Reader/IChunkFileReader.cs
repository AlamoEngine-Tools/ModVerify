using System;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Data;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

/// <summary>
/// Reads a chunked file from a stream and produces a parsed representation.
/// </summary>
public interface IChunkFileReader : IDisposable
{
    /// <summary>
    /// Reads the chunked file and returns its parsed data.
    /// </summary>
    /// <returns>The parsed chunk data.</returns>
    /// <exception cref="BinaryCorruptedException">The stream contains invalid or unexpected data.</exception>
    IChunkData Read();
}

/// <summary>
/// Reads a chunked file from a stream and produces a strongly-typed parsed representation.
/// </summary>
/// <typeparam name="T">The type of chunk data produced by this reader.</typeparam>
public interface IChunkFileReader<out T> : IChunkFileReader where T : IChunkData
{
    /// <summary>
    /// Reads the chunked file and returns its parsed data.
    /// </summary>
    /// <returns>The parsed chunk data of type <typeparamref name="T"/>.</returns>
    /// <exception cref="BinaryCorruptedException">The stream contains invalid or unexpected data.</exception>
    new T Read();
}