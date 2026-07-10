using System.Runtime.CompilerServices;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model.Metadata;

public class MiniChunkMetadataTest
{
    [Fact]
    public void SizeOf_Is2Bytes()
    {
        Assert.Equal(2, Unsafe.SizeOf<MiniChunkMetadata>());
    }

    [Fact]
    public void Ctor_SetsTypeAndBodySize()
    {
        var meta = new MiniChunkMetadata(0x0A, 0xFF);
        Assert.Equal(0x0A, meta.Type);
        Assert.Equal(0xFF, meta.BodySize);
    }

    [Fact]
    public void Default_HasZeroValues()
    {
        var meta = default(MiniChunkMetadata);
        Assert.Equal(0, meta.Type);
        Assert.Equal(0, meta.BodySize);
    }

    [Fact]
    public void Ctor_MaxValues()
    {
        var meta = new MiniChunkMetadata(byte.MaxValue, byte.MaxValue);
        Assert.Equal(byte.MaxValue, meta.Type);
        Assert.Equal(byte.MaxValue, meta.BodySize);
    }
}
