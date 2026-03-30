using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// A chunk containing mini-chunk children.
/// </summary>
/// <remarks>
/// <para>
/// This chunk type holds exclusively <see cref="MiniChunk"/> children.
/// Bit 31 of the size field is not used as a container flag.
/// </para>
/// </remarks>
public sealed class MiniNodeChunk : NodeChunkBase<MiniChunk>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MiniNodeChunk"/> class.
    /// </summary>
    /// <param name="info">The chunk metadata. Must not have bit 31 set.</param>
    /// <param name="children">The mini-chunk children. Must contain at least one element.</param>
    /// <exception cref="NotSupportedException">Chunks larger than 2GB are not supported.</exception>
    public MiniNodeChunk(ChunkMetadata info, IReadOnlyList<MiniChunk> children) : base(info, children)
    {
        if (info.RawSize > int.MaxValue)
            throw new NotSupportedException("Chunks larger than int32.MaxValue bytes are not supported.");
    }
}