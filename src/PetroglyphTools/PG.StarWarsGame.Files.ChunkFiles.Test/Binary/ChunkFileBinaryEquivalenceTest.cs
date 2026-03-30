using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary;

public class ChunkFileBinaryEquivalenceTest
{
    [Fact]
    public void StructuredFile_And_RawFile_ProduceSameBinary()
    {
        var structuredBytes = TestChunkFileData.StructuredFile.Bytes;
        var rawBytes = TestChunkFileData.RawFile.Bytes;

        Assert.Equal(structuredBytes, rawBytes);
    }

    [Fact]
    public void StructuredFile_MatchesExpectedBytes()
    {
        Assert.Equal(TestChunkFileData.ExpectedBytes, TestChunkFileData.StructuredFile.Bytes);
    }

    [Fact]
    public void RawFile_MatchesExpectedBytes()
    {
        Assert.Equal(TestChunkFileData.ExpectedBytes, TestChunkFileData.RawFile.Bytes);
    }

    [Fact]
    public void StructuredFile_HasExpectedRootCount()
    {
        Assert.Equal(3, TestChunkFileData.StructuredFile.RootChunks.Count);
    }

    [Fact]
    public void RawFile_HasExpectedRootCount()
    {
        Assert.Equal(3, TestChunkFileData.RawFile.RootChunks.Count);
    }

    [Fact]
    public void Files_HaveSameSize()
    {
        Assert.Equal(TestChunkFileData.StructuredFile.Size, TestChunkFileData.RawFile.Size);
    }
}
