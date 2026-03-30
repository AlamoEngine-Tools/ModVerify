using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Reader;

public sealed class TestChunkFileReader(Stream stream, bool leaveStreamOpen = false)
    : ChunkFileReaderBase<TestChunkFileReaderTest.TestChunkData>(stream, leaveStreamOpen)
{
    public override TestChunkFileReaderTest.TestChunkData Read()
    {
        var meta = ChunkReader.ReadChunk();
        var data = ChunkReader.ReadData(meta);
        return new TestChunkFileReaderTest.TestChunkData(data);
    }

    public void CallThrowIfChunkSizeTooLarge(ChunkMetadata chunk)
    {
        ThrowIfChunkSizeTooLargeException(chunk);
    }
}