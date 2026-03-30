namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Model;

/// <summary>
/// Base class for chunks that can appear as root-level elements in a <see cref="ChunkFile"/>
/// or as children in a <see cref="NodeChunk"/>.
/// </summary>
public abstract class RootChunk : Chunk;