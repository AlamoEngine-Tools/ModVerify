using System;
using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using PG.StarWarsGame.Files.ChunkFiles.Data;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Reader;

public abstract class ChunkFileReaderBaseTest<TReader, TData>
    where TReader : ChunkFileReaderBase<TData>
    where TData : IChunkData
{
    protected abstract TReader CreateReader(Stream stream, bool leaveStreamOpen = false);

    protected abstract byte[] CreateValidStreamContent();

    protected abstract void AssertReadResult(TData result);

    protected abstract void CallThrowIfChunkSizeTooLarge(TReader reader, ChunkMetadata chunk);

    [Fact]
    public void Read_ReturnsChunkData()
    {
        var content = CreateValidStreamContent();
        using var reader = CreateReader(new MemoryStream(content));
        var result = reader.Read();
        AssertReadResult(result);
    }

    [Fact]
    public void Read_IChunkFileReader_ReturnsIChunkData()
    {
        var content = CreateValidStreamContent();
        using var reader = CreateReader(new MemoryStream(content));
        IChunkFileReader interfaceReader = reader;
        var result = interfaceReader.Read();
        Assert.IsType<TData>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dispose_StreamBehaviorDependsOnLeaveOpen(bool leaveOpen)
    {
        var ms = new MemoryStream([1, 2, 3]);
        var reader = CreateReader(ms, leaveStreamOpen: leaveOpen);
        reader.Dispose();

        if (leaveOpen)
        {
            ms.Position = 0;
            Assert.Equal(1, ms.ReadByte());
        }
        else
        {
            Assert.Throws<ObjectDisposedException>(() => ms.ReadByte());
        }
    }

    [Fact]
    public void ThrowIfChunkSizeTooLarge_DoesNotThrow_ForNormalSize()
    {
        using var reader = CreateReader(new MemoryStream(new byte[8]));
        var meta = new ChunkMetadata(1, 100);
        CallThrowIfChunkSizeTooLarge(reader, meta);
    }

    [Fact]
    public void ThrowIfChunkSizeTooLarge_Throws_WhenRawSizeExceedsIntMax()
    {
        using var reader = CreateReader(new MemoryStream(new byte[8]));
        var meta = new ChunkMetadata(1, 0x8000_0001u);
        Assert.Throws<NotSupportedException>(() => CallThrowIfChunkSizeTooLarge(reader, meta));
    }

    [Fact]
    public void ThrowIfChunkSizeTooLarge_DoesNotThrow_AtExactIntMax()
    {
        using var reader = CreateReader(new MemoryStream(new byte[8]));
        var meta = new ChunkMetadata(1, int.MaxValue);
        CallThrowIfChunkSizeTooLarge(reader, meta);
    }
}