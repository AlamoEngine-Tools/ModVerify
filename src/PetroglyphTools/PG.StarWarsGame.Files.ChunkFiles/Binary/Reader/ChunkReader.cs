using System;
using System.IO;
using System.Text;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

/// <summary>
/// Reads chunks and their data from a stream.
/// </summary>
/// <remarks>
/// <para>
/// The reader operates sequentially on the underlying stream. It reads chunk headers,
/// mini-chunk headers, and raw data. It does not build a tree structure — callers are
/// responsible for interpreting chunk relationships based on the file format specification.
/// </para>
/// <para>
/// Several methods accept a <c>ref int readBytes</c> parameter that tracks the number
/// of bytes read. This is useful for parsing chunk bodies where the caller needs to know
/// when the body has been fully consumed.
/// </para>
/// </remarks>
public class ChunkReader : DisposableObject
{
    private readonly PetroglyphBinaryReader _binaryReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkReader"/> class.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="leaveOpen">
    /// <see langword="true"/> to leave the stream open after this reader is disposed; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
    public ChunkReader(Stream stream, bool leaveOpen = false)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        _binaryReader = new PetroglyphBinaryReader(stream, leaveOpen);
    }

    /// <summary>
    /// Reads an 8-byte chunk header from the stream.
    /// </summary>
    /// <returns>The chunk metadata.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the header could be read.</exception>
    public ChunkMetadata ReadChunk()
    {
        var type = _binaryReader.ReadUInt32();
        var rawSize = _binaryReader.ReadUInt32();
        return new ChunkMetadata(type, rawSize);
    }

    /// <summary>
    /// Reads an 8-byte chunk header from the stream and advances the byte counter.
    /// </summary>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by <value>8</value>.
    /// </param>
    /// <returns>The chunk metadata.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the header could be read.</exception>
    public ChunkMetadata ReadChunk(ref int readBytes)
    {
        var chunk = ReadChunk();
        IncrementReadBytesByChunkSize(ref readBytes);
        return chunk;
    }

    /// <summary>
    /// Reads a 2-byte mini-chunk header from the stream and advances the byte counter.
    /// </summary>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by <value>2</value>.
    /// </param>
    /// <returns>The mini-chunk metadata.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the header could be read.</exception>
    public MiniChunkMetadata ReadMiniChunk(ref int readBytes)
    {
        var type = _binaryReader.ReadByte();
        var size = _binaryReader.ReadByte();
        IncrementReadBytesByMiniChunkSize(ref readBytes);
        return new MiniChunkMetadata(type, size);
    }

    /// <summary>
    /// Reads the body data of a chunk.
    /// </summary>
    /// <param name="chunk">The chunk whose body to read.</param>
    /// <returns>The chunk body as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Attempt to read data from a node chunk.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the body could be read.</exception>
    public byte[] ReadData(ChunkMetadata chunk)
    {
        if (chunk.RawSize > int.MaxValue)
            throw new InvalidOperationException("Cannot to read data from container chunk.");
        return _binaryReader.ReadBytes(chunk.BodySize);
    }

    /// <summary>
    /// Reads the body data of a mini chunk.
    /// </summary>
    /// <param name="chunk">The mini chunk whose body to read.</param>
    /// <returns>The chunk body as a byte array.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the body could be read.</exception>
    public byte[] ReadData(MiniChunkMetadata chunk)
    {
        return _binaryReader.ReadBytes(chunk.BodySize);
    }

    /// <summary>
    /// Reads the specified number of bytes from the stream.
    /// </summary>
    /// <param name="size">The number of bytes to read.</param>
    /// <returns>The data as a byte array.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is negative.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before all bytes could be read.</exception>
    public byte[] ReadData(int size)
    {
        return size < 0
            ? throw new ArgumentOutOfRangeException(nameof(size), "size cannot be negative")
            : _binaryReader.ReadBytes(size);
    }

    /// <summary>
    /// Reads the body data of a chunk and advances the byte counter.
    /// </summary>
    /// <param name="chunk">The chunk whose body to read.</param>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by the body size of <paramref name="chunk"/>.
    /// </param>
    /// <returns>The chunk body as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Attempt to read data from a node chunk.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the body could be read.</exception>
    public byte[] ReadData(ChunkMetadata chunk, ref int readBytes)
    {
        if (chunk.RawSize >= int.MaxValue)
            throw new InvalidOperationException("Cannot to read data from container chunk.");

        var data = _binaryReader.ReadBytes(chunk.BodySize);
        readBytes += chunk.BodySize;
        return data;
    }

    /// <summary>
    /// Reads the body data of a mini chunk and advances the byte counter.
    /// </summary>
    /// <param name="chunk">The mini chunk whose body to read.</param>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by the body size of <paramref name="chunk"/>.
    /// </param>
    /// <returns>The chunk body as a byte array.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the body could be read.</exception>
    public byte[] ReadData(MiniChunkMetadata chunk, ref int readBytes)
    {
        var data = _binaryReader.ReadBytes(chunk.BodySize);
        readBytes += chunk.BodySize;
        return data;
    }

    /// <summary>
    /// Reads a 4-byte unsigned integer from the stream and advances the byte counter.
    /// </summary>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by <value>4</value>.
    /// </param>
    /// <returns>The value read.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the value could be read.</exception>
    public uint ReadDword(ref int readBytes)
    {
        var value = _binaryReader.ReadUInt32();
        readBytes += sizeof(uint);
        return value;
    }

    /// <summary>
    /// Reads a 4-byte floating-point value from the stream and advances the byte counter.
    /// </summary>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by <value>4</value>.
    /// </param>
    /// <returns>The value read.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the value could be read.</exception>
    public float ReadFloat(ref int readBytes)
    {
        var value = _binaryReader.ReadSingle();
        readBytes += sizeof(float);
        return value;
    }

    /// <summary>
    /// Reads a 4-byte unsigned integer from the stream.
    /// </summary>
    /// <returns>The value read.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before the value could be read.</exception>
    public uint ReadDword()
    {
        return _binaryReader.ReadUInt32();
    }

    /// <summary>
    /// Advances the stream position by the specified number of bytes
    /// and advances the byte counter.
    /// </summary>
    /// <param name="bytesToSkip">The number of bytes to skip.</param>
    /// <param name="readBytes">Incremented by <paramref name="bytesToSkip"/>.</param>
    public void Skip(int bytesToSkip, ref int readBytes)
    {
        _binaryReader.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
        readBytes += bytesToSkip;
    }

    /// <summary>
    /// Advances the stream position by the specified number of bytes.
    /// </summary>
    /// <param name="bytesToSkip">The number of bytes to skip.</param>
    public void Skip(int bytesToSkip)
    {
        _binaryReader.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
    }

    /// <summary>
    /// Reads a string from the stream and advances the byte counter.
    /// </summary>
    /// <param name="size">The number of bytes to read for the string.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="zeroTerminated">
    /// <see langword="true"/> to trim the string at the first null character;otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns, it is incremented by <paramref name="size"/>.
    /// </param>
    /// <returns>The decoded string.</returns>
    /// <exception cref="BinaryCorruptedException">The string data could not be decoded.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before all bytes could be read.</exception>
    public string ReadString(int size, Encoding encoding, bool zeroTerminated, ref int readBytes)
    {
        var value = ReadString(encoding, size, zeroTerminated);
        readBytes += size;
        return value;
    }

    /// <summary>
    /// Reads a string from the stream.
    /// </summary>
    /// <param name="size">The number of bytes to read for the string.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="zeroTerminated">
    /// <see langword="true"/> to trim the string at the first null character;otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The decoded string.</returns><exception cref="BinaryCorruptedException">The string data could not be decoded.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream is reached before all bytes could be read.</exception>
    public string ReadString(int size, Encoding encoding, bool zeroTerminated)
    {
        var value = ReadString(encoding, size, zeroTerminated);
        return value;
    }

    /// <summary>
    /// Reads a chunk header from the stream if data is available.
    /// </summary>
    /// <returns>
    /// The chunk metadata, or <see langword="null"/> if the stream is at the end.
    /// </returns>
    public ChunkMetadata? TryReadChunk()
    {
        var _ = 0;
        return TryReadChunk(ref _);
    }

    /// <summary>
    /// Reads a chunk header from the stream if data is available
    /// and advances the byte counter.
    /// </summary>
    /// <param name="readBytes">
    /// Holds the number of bytes read so far.
    /// When this method returns and a chunk was read, it is incremented by <value>8</value>.
    /// </param>
    /// <returns>
    /// The chunk metadata, or <see langword="null"/> if the stream is at the end.
    /// </returns>
    public ChunkMetadata? TryReadChunk(ref int readBytes)
    {
        if (_binaryReader.BaseStream.Position == _binaryReader.BaseStream.Length)
            return null;

        var chunk = ReadChunk();
        IncrementReadBytesByChunkSize(ref readBytes);
        return chunk;
    }

    /// <summary>
    /// Releases the resources used by the <see cref="ChunkReader"/>.
    /// </summary>
    protected override void DisposeResources()
    {
        base.DisposeResources();
        _binaryReader.Dispose();
    }

    private static unsafe void IncrementReadBytesByChunkSize(ref int readBytes)
    {
        readBytes += sizeof(ChunkMetadata);
    }

    private static unsafe void IncrementReadBytesByMiniChunkSize(ref int readBytes)
    {
        readBytes += sizeof(MiniChunkMetadata);
    }

    private string ReadString(Encoding encoding, int size, bool zeroTerminated)
    {
        try
        {
            return _binaryReader.ReadString(encoding, size, zeroTerminated);
        }
        catch (Exception e)
        {
            throw new BinaryCorruptedException($"Unable to read string: {e.Message}", e);
        }
    }
}