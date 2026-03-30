using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Data;
using System;
using System.IO;
using Xunit;
using static PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Reader.TestChunkFileReaderTest;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Reader;

public sealed class TestChunkFileReaderTest : ChunkFileReaderBaseTest<TestChunkFileReader, TestChunkData>
{
    public sealed class TestChunkData(byte[] data) : IChunkData
    {
        public byte[] Data { get; } = data;

        public void Dispose() { }
    }

    protected override TestChunkFileReader CreateReader(Stream stream, bool leaveStreamOpen = false)
    {
        return new TestChunkFileReader(stream, leaveStreamOpen);
    }

    protected override byte[] CreateValidStreamContent()
    {
        var header = new byte[8];
        BitConverter.GetBytes(0x01u).CopyTo(header, 0);
        BitConverter.GetBytes(3u).CopyTo(header, 4);
        var body = new byte[] { 0xAA, 0xBB, 0xCC };
        var stream = new byte[header.Length + body.Length];
        header.CopyTo(stream, 0);
        body.CopyTo(stream, header.Length);
        return stream;
    }

    protected override void AssertReadResult(TestChunkData result)
    {
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC }, result.Data);
    }

    protected override void CallThrowIfChunkSizeTooLarge(TestChunkFileReader reader, ChunkMetadata chunk)
    {
        reader.CallThrowIfChunkSizeTooLarge(chunk);
    }
}