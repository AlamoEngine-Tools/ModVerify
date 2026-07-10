using System.Runtime.CompilerServices;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Model.Metadata;

public class ChunkMetadataTest
{
    [Fact]
    public void SizeOf_Is8Bytes()
    {
        Assert.Equal(8, Unsafe.SizeOf<ChunkMetadata>());
    }

    [Fact]
    public void Ctor_SetsTypeAndRawSize()
    {
        var meta = new ChunkMetadata(0x1234u, 0x5678u);
        Assert.Equal(0x1234u, meta.Type);
        Assert.Equal(0x5678u, meta.RawSize);
    }

    [Fact]
    public void Default_HasZeroValues()
    {
        var meta = default(ChunkMetadata);
        Assert.Equal(0u, meta.Type);
        Assert.Equal(0u, meta.RawSize);
        Assert.False(meta.HasChildrenHint);
        Assert.Equal(0, meta.BodySize);
        Assert.Equal((uint)meta.BodySize, meta.RawSize);
    }

    [Fact]
    public void HasChildrenHint_ReturnsFalse_WhenBit31NotSet()
    {
        var meta = new ChunkMetadata(1, 0x7FFF_FFFFu);
        Assert.Equal(0x7FFF_FFFFu, meta.RawSize);
        Assert.False(meta.HasChildrenHint);
    }

    [Fact]
    public void HasChildrenHint_ReturnsTrue_WhenBit31Set()
    {
        var meta = new ChunkMetadata(1, 0x8000_0000u);
        Assert.Equal(0x8000_0000u, meta.RawSize);
        Assert.True(meta.HasChildrenHint);
    }

    [Fact]
    public void HasChildrenHint_ReturnsTrue_WhenAllBitsSet()
    {
        var meta = new ChunkMetadata(1, 0xFFFF_FFFFu);
        Assert.Equal(0xFFFF_FFFFu, meta.RawSize);
        Assert.True(meta.HasChildrenHint);
    }

    [Fact]
    public void BodySize_MasksBit31Off()
    {
        var meta = new ChunkMetadata(1, 0x8000_0005u);
        Assert.Equal(0x8000_0005u, meta.RawSize);
        Assert.Equal(5, meta.BodySize);
    }

    [Fact]
    public void BodySize_ReturnsRawSize_WhenBit31NotSet()
    {
        var meta = new ChunkMetadata(1, 100u);
        Assert.Equal(100u, meta.RawSize);
        Assert.Equal(100, meta.BodySize);
        Assert.Equal((uint)meta.BodySize, meta.RawSize);
    }

    [Fact]
    public void BodySize_ReturnsMaxValue_WhenAllLower31BitsSet()
    {
        var meta = new ChunkMetadata(1, 0xFFFF_FFFFu);
        Assert.Equal(0xFFFF_FFFFu, meta.RawSize);
        Assert.Equal(int.MaxValue, meta.BodySize);
    }

    [Fact]
    public void BodySize_ReturnsZero_WhenOnlyBit31Set()
    {
        var meta = new ChunkMetadata(1, 0x8000_0000u);
        Assert.Equal(0x8000_0000u, meta.RawSize);
        Assert.Equal(0, meta.BodySize);
    }
}
