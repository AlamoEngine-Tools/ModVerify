using System;
using System.IO;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Data;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

/// <summary>
/// Represents the base class for reading chunk files in a binary format.
/// </summary>
/// <typeparam name="T">The type of chunk data being read.</typeparam>
/// <param name="stream">The input stream from which the chunk file is read.</param>
/// <param name="leaveStreamOpen">
/// A boolean value indicating whether the input stream should remain open after the reader is disposed.
/// Default value is <see langword="false"/>.
/// </param>
public abstract class ChunkFileReaderBase<T>(Stream stream, bool leaveStreamOpen = false) 
    : DisposableObject, IChunkFileReader<T> where T : IChunkData
{
    /// <summary>
    /// The <see cref="ChunkReader"/> instance used to read chunk data from the input stream.
    /// </summary>
    /// <remarks>
    /// This reader is initialized with the provided stream and the option to leave the stream open after disposal.
    /// </remarks>
    protected readonly ChunkReader ChunkReader = new(stream, leaveStreamOpen);

    /// <inheritdoc/>
    public abstract T Read();

    /// <inheritdoc/>
    IChunkData IChunkFileReader.Read()
    {
        return Read();
    }

    /// <summary>
    /// Releases the resources used by the <see cref="ChunkFileReaderBase{T}"/> instance, including
    /// the associated <see cref="ChunkReader"/>.
    /// </summary>
    protected override void DisposeResources()
    {
        base.DisposeResources();
        ChunkReader.Dispose();
    }

    /// <summary>
    /// Validates the raw binary size of the specified chunk and throws an exception if the size exceeds the maximum allowed value.
    /// </summary>
    /// <remarks>
    /// This method should not be used for <see cref="NodeChunk"/>, as their raw size always exceeds <see cref="int.MaxValue"/>
    /// due to the presence of the high bit flag in the size field, which indicates that the chunk has child chunks.
    /// </remarks>
    /// <param name="chunk">The metadata of the chunk to validate.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when the raw binary size of the chunk exceeds <see cref="int.MaxValue"/>, as such sizes are not supported.
    /// </exception>
    protected void ThrowIfChunkSizeTooLargeException(ChunkMetadata chunk)
    {
        if (chunk.RawSize > int.MaxValue) 
            throw new NotSupportedException("Chunk sizes larger than int.MaxValue are not supported.");
    }
}