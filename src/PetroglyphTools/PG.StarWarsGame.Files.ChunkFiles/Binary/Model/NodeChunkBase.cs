using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// Base class for chunks that contain child chunks of type <typeparamref name="TChild"/>.
/// </summary>
/// <typeparam name="TChild">The type of child chunk this node contains.</typeparam>
public abstract class NodeChunkBase<TChild> : RootChunk where TChild : Chunk
{
    /// <summary>
    /// Gets the chunk metadata.
    /// </summary>
    public ChunkMetadata Info { get; }

    /// <summary>
    /// Gets the child chunks.
    /// </summary>
    public IReadOnlyList<TChild> Children { get; }

    /// <inheritdoc />
    public override unsafe int Size => sizeof(ChunkMetadata) + Children.Sum(c => c.Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeChunkBase{TChild}"/> class.
    /// </summary>
    /// <param name="info">The chunk metadata.</param>
    /// <param name="children">The child chunks.</param>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="info"/> body size does not match the sum of children sizes.
    /// </exception>
    protected NodeChunkBase(ChunkMetadata info, IReadOnlyList<TChild> children)
    {
        if (children == null)
            throw new ArgumentNullException(nameof(children));

        var actualSize = children.Sum(c => c.Size);
        if (info.BodySize != actualSize)
            throw new ArgumentException(
                $"Metadata size ({info.BodySize}) does not match sum of children sizes ({actualSize}).",
                nameof(info));

        Info = info;
        Children = children;
    }

    /// <inheritdoc />
    public override unsafe void GetBytes(Span<byte> bytes)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Info.Type);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[sizeof(uint)..], Info.RawSize);

        var offset = sizeof(ChunkMetadata);
        foreach (var child in Children)
        {
            child.GetBytes(bytes[offset..]);
            offset += child.Size;
        }
    }
}