using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// A chunk containing regular child chunks.
/// </summary>
/// <remarks>
/// Bit 31 of the size field must be set.
/// </remarks>
public sealed class NodeChunk : NodeChunkBase<RootChunk>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeChunk"/> class.
    /// </summary>
    /// <param name="info">The chunk metadata. Must have bit 31 set.</param>
    /// <param name="children">The child chunks.</param>
    /// <exception cref="ArgumentException"><paramref name="info"/> does not have bit 31 set.</exception>
    public NodeChunk(ChunkMetadata info, IReadOnlyList<RootChunk> children) : base(info, children)
    {
        if (!info.HasChildrenHint)
            throw new ArgumentException(
                "NodeChunk metadata must have bit 31 set.", nameof(info));
    }
}