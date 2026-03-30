using System;
using System.IO;
using System.Text;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using Xunit;

namespace PG.StarWarsGame.Files.ChunkFiles.Test.Binary.Reader;

public class ChunkReaderTest
{
    private static MemoryStream CreateStream(byte[] data) => new(data);

    private static byte[] WriteChunkHeader(uint type, uint rawSize)
    {
        var bytes = new byte[8];
        BitConverter.GetBytes(type).CopyTo(bytes, 0);
        BitConverter.GetBytes(rawSize).CopyTo(bytes, 4);
        return bytes;
    }

    [Fact]
    public void Ctor_ThrowsOnNullStream()
    {
        Assert.Throws<ArgumentNullException>(() => new ChunkReader(null!));
    }

    [Fact]
    public void ReadChunk_ReadsHeaderCorrectly()
    {
        var header = WriteChunkHeader(0x01, 0x0A);
        using var reader = new ChunkReader(CreateStream(header));

        var meta = reader.ReadChunk();
        Assert.Equal(0x01u, meta.Type);
        Assert.Equal(0x0Au, meta.RawSize);
    }

    [Fact]
    public void ReadChunk_WithRefBytes_IncrementsBy8()
    {
        var header = WriteChunkHeader(0x01, 0x0A);
        using var reader = new ChunkReader(CreateStream(header));

        var readBytes = 0;
        var meta = reader.ReadChunk(ref readBytes);
        Assert.Equal(8, readBytes);
        Assert.Equal(0x01u, meta.Type);
    }

    [Fact]
    public void ReadChunk_ThrowsAtEndOfStream()
    {
        using var reader = new ChunkReader(CreateStream(Array.Empty<byte>()));
        Assert.Throws<EndOfStreamException>(() => reader.ReadChunk());
    }

    [Fact]
    public void ReadMiniChunk_ReadsHeaderCorrectly()
    {
        var data = new byte[] { 0x05, 0x03 };
        using var reader = new ChunkReader(CreateStream(data));

        var readBytes = 0;
        var meta = reader.ReadMiniChunk(ref readBytes);
        Assert.Equal(0x05, meta.Type);
        Assert.Equal(0x03, meta.BodySize);
        Assert.Equal(2, readBytes);
    }

    [Fact]
    public void ReadData_ChunkMetadata_ReadsBody()
    {
        var header = WriteChunkHeader(0x01, 3);
        var body = new byte[] { 0xAA, 0xBB, 0xCC };
        var stream = new byte[header.Length + body.Length];
        header.CopyTo(stream, 0);
        body.CopyTo(stream, header.Length);

        using var reader = new ChunkReader(CreateStream(stream));
        var meta = reader.ReadChunk();
        var data = reader.ReadData(meta);

        Assert.Equal(body, data);
    }

    [Fact]
    public void ReadData_ChunkMetadata_ThrowsForContainerChunk()
    {
        var header = WriteChunkHeader(0x01, 0x8000_0003u);
        using var reader = new ChunkReader(CreateStream(header));
        var meta = reader.ReadChunk();

        Assert.Throws<InvalidOperationException>(() => reader.ReadData(meta));
    }

    [Fact]
    public void ReadData_ChunkMetadata_WithRefBytes_IncrementsCorrectly()
    {
        var header = WriteChunkHeader(0x01, 3);
        var body = new byte[] { 0xAA, 0xBB, 0xCC };
        var stream = new byte[header.Length + body.Length];
        header.CopyTo(stream, 0);
        body.CopyTo(stream, header.Length);

        using var reader = new ChunkReader(CreateStream(stream));
        var meta = reader.ReadChunk();
        var readBytes = 0;
        var data = reader.ReadData(meta, ref readBytes);

        Assert.Equal(body, data);
        Assert.Equal(3, readBytes);
    }

    [Fact]
    public void ReadData_MiniChunkMetadata_ReadsBody()
    {
        var miniHeader = new byte[] { 0x05, 0x02 };
        var body = new byte[] { 0xDD, 0xEE };
        var stream = new byte[miniHeader.Length + body.Length];
        miniHeader.CopyTo(stream, 0);
        body.CopyTo(stream, miniHeader.Length);

        using var reader = new ChunkReader(CreateStream(stream));
        var rb = 0;
        var meta = reader.ReadMiniChunk(ref rb);
        var data = reader.ReadData(meta);

        Assert.Equal(body, data);
    }

    [Fact]
    public void ReadData_MiniChunkMetadata_WithRefBytes_IncrementsCorrectly()
    {
        var miniHeader = new byte[] { 0x05, 0x02 };
        var body = new byte[] { 0xDD, 0xEE };
        var stream = new byte[miniHeader.Length + body.Length];
        miniHeader.CopyTo(stream, 0);
        body.CopyTo(stream, miniHeader.Length);

        using var reader = new ChunkReader(CreateStream(stream));
        var rb = 0;
        var meta = reader.ReadMiniChunk(ref rb);
        rb = 0;
        var data = reader.ReadData(meta, ref rb);

        Assert.Equal(body, data);
        Assert.Equal(2, rb);
    }

    [Fact]
    public void ReadData_BySize_ReadsCorrectBytes()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        using var reader = new ChunkReader(CreateStream(data));

        var result = reader.ReadData(3);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
    }

    [Fact]
    public void ReadData_BySize_ThrowsOnNegative()
    {
        using var reader = new ChunkReader(CreateStream(new byte[] { 0x01 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadData(-1));
    }

    [Fact]
    public void ReadDword_ReadsUInt32()
    {
        var data = BitConverter.GetBytes(0x12345678u);
        using var reader = new ChunkReader(CreateStream(data));

        var value = reader.ReadDword();
        Assert.Equal(0x12345678u, value);
    }

    [Fact]
    public void ReadDword_WithRefBytes_IncrementsBy4()
    {
        var data = BitConverter.GetBytes(42u);
        using var reader = new ChunkReader(CreateStream(data));

        var readBytes = 0;
        var value = reader.ReadDword(ref readBytes);
        Assert.Equal(42u, value);
        Assert.Equal(4, readBytes);
    }

    [Fact]
    public void ReadFloat_WithRefBytes_IncrementsBy4()
    {
        var data = BitConverter.GetBytes(3.14f);
        using var reader = new ChunkReader(CreateStream(data));

        var readBytes = 0;
        var value = reader.ReadFloat(ref readBytes);
        Assert.Equal(3.14f, value);
        Assert.Equal(4, readBytes);
    }

    [Fact]
    public void Skip_AdvancesStreamPosition()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        using var reader = new ChunkReader(CreateStream(data));

        reader.Skip(3);
        var result = reader.ReadData(2);
        Assert.Equal(new byte[] { 0x04, 0x05 }, result);
    }

    [Fact]
    public void Skip_WithRefBytes_IncrementsCounter()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };
        using var reader = new ChunkReader(CreateStream(data));

        var readBytes = 0;
        reader.Skip(2, ref readBytes);
        Assert.Equal(2, readBytes);
    }

    [Fact]
    public void TryReadChunk_ReturnsNull_AtEndOfStream()
    {
        using var reader = new ChunkReader(CreateStream(Array.Empty<byte>()));
        var result = reader.TryReadChunk();
        Assert.Null(result);
    }

    [Fact]
    public void TryReadChunk_ReturnsMetadata_WhenDataAvailable()
    {
        var header = WriteChunkHeader(0x42, 0x10);
        using var reader = new ChunkReader(CreateStream(header));

        var result = reader.TryReadChunk();
        Assert.NotNull(result);
        Assert.Equal(0x42u, result.Value.Type);
        Assert.Equal(0x10u, result.Value.RawSize);
    }

    [Fact]
    public void TryReadChunk_WithRefBytes_IncrementsBy8()
    {
        var header = WriteChunkHeader(0x42, 0x10);
        using var reader = new ChunkReader(CreateStream(header));

        var readBytes = 0;
        var result = reader.TryReadChunk(ref readBytes);
        Assert.NotNull(result);
        Assert.Equal(8, readBytes);
    }

    [Fact]
    public void TryReadChunk_WithRefBytes_DoesNotIncrement_AtEnd()
    {
        using var reader = new ChunkReader(CreateStream(Array.Empty<byte>()));

        var readBytes = 5;
        var result = reader.TryReadChunk(ref readBytes);
        Assert.Null(result);
        Assert.Equal(5, readBytes);
    }

    [Fact]
    public void ReadString_ReadsCorrectly()
    {
        var text = "Hello";
        var encoded = Encoding.ASCII.GetBytes(text);
        using var reader = new ChunkReader(CreateStream(encoded));

        var readBytes = 0;
        var result = reader.ReadString(encoded.Length, Encoding.ASCII, false, ref readBytes);
        Assert.Equal(text, result);
        Assert.Equal(encoded.Length, readBytes);
    }

    [Fact]
    public void ReadString_ZeroTerminated_TrimsAtNull()
    {
        var encoded = new byte[] { (byte)'H', (byte)'i', 0, (byte)'X' };
        using var reader = new ChunkReader(CreateStream(encoded));

        var result = reader.ReadString(encoded.Length, Encoding.ASCII, true);
        Assert.Equal("Hi", result);
    }

    [Fact]
    public void ReadString_WithoutRefBytes_Works()
    {
        var text = "AB";
        var encoded = Encoding.ASCII.GetBytes(text);
        using var reader = new ChunkReader(CreateStream(encoded));

        var result = reader.ReadString(encoded.Length, Encoding.ASCII, false);
        Assert.Equal(text, result);
    }

    [Fact]
    public void Dispose_DisposesUnderlyingStream()
    {
        var ms = CreateStream(new byte[] { 1, 2, 3 });
        var reader = new ChunkReader(ms);
        reader.Dispose();

        Assert.Throws<ObjectDisposedException>(() => ms.ReadByte());
    }

    [Fact]
    public void Dispose_LeaveOpen_KeepsStreamOpen()
    {
        var ms = CreateStream(new byte[] { 1, 2, 3 });
        var reader = new ChunkReader(ms, leaveOpen: true);
        reader.Dispose();

        // Stream should still be usable
        ms.Position = 0;
        Assert.Equal(1, ms.ReadByte());
    }

    [Fact]
    public void ReadChunk_Roundtrip_WithChunkFactory()
    {
        var original = PG.StarWarsGame.Files.ChunkFiles.Binary.ChunkFactory.Data(0x42, new byte[] { 1, 2, 3 });
        var bytes = original.Bytes;

        using var reader = new ChunkReader(CreateStream(bytes));
        var meta = reader.ReadChunk();
        var data = reader.ReadData(meta);

        Assert.Equal(0x42u, meta.Type);
        Assert.Equal(3, meta.BodySize);
        Assert.False(meta.HasChildrenHint);
        Assert.Equal(new byte[] { 1, 2, 3 }, data);
    }
}
