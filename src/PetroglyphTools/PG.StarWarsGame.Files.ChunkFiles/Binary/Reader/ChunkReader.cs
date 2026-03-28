using System;
using System.IO;
using System.Text;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;

public class ChunkReader : DisposableObject
{
    private readonly PetroglyphBinaryReader _binaryReader;

    public ChunkReader(Stream stream, bool leaveOpen = false)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        _binaryReader = new PetroglyphBinaryReader(stream, leaveOpen);
    }

    public ChunkMetadata ReadChunk()
    {
        var type = _binaryReader.ReadUInt32();
        var rawSize = _binaryReader.ReadUInt32();
        return new ChunkMetadata(type, rawSize, false);
    }

    public ChunkMetadata ReadChunk(ref int readBytes)
    {
        var chunk = ReadChunk();
        readBytes += 8;
        return chunk;
    }

    public ChunkMetadata ReadMiniChunk(ref int readBytes)
    {
        var type = _binaryReader.ReadByte();
        var size = _binaryReader.ReadByte();

        readBytes += 2;

        return new ChunkMetadata(type, size, true);
    }

    public byte[] ReadData(ChunkMetadata chunk)
    {
        if (chunk.HasChildrenHint)
            throw new InvalidOperationException("Unable to read data from container chunk.");

        return _binaryReader.ReadBytes(chunk.BodySize);
    }

    public byte[] ReadData(int size)
    {
        return size < 0 ?
            throw new ArgumentOutOfRangeException(nameof(size), "size cannot be negative") : 
            _binaryReader.ReadBytes(size);
    }

    public byte[] ReadData(ChunkMetadata chunk, ref int readSize)
    {
        if (chunk.HasChildrenHint)
            throw new InvalidOperationException("Unable to read data from container chunk.");

        var data = _binaryReader.ReadBytes(chunk.BodySize);
        readSize += chunk.BodySize;
        return data;
    }


    public uint ReadDword(ref int readSize)
    {
        var value = _binaryReader.ReadUInt32();
        readSize += sizeof(uint);
        return value;
    }

    public float ReadFloat(ref int readSize)
    {
        var value = _binaryReader.ReadSingle();
        readSize += sizeof(float);
        return value;
    }

    public uint ReadDword()
    { 
        return _binaryReader.ReadUInt32();
    }

    public void Skip(int bytesToSkip, ref int readBytes)
    {
        _binaryReader.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
        readBytes += bytesToSkip;
    }

    public void Skip(int bytesToSkip)
    {
        _binaryReader.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
    }

    public string ReadString(int size, Encoding encoding, bool zeroTerminated, ref int readSize)
    {
        var value = ReadString(encoding, size, zeroTerminated);
        readSize += size;
        return value;
    }

    public string ReadString(int size, Encoding encoding, bool zeroTerminated)
    {
        var value = ReadString(encoding, size, zeroTerminated);
        return value;
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

    public ChunkMetadata? TryReadChunk()
    {
        var _ = 0;
        return TryReadChunk(ref _);
    }

    public ChunkMetadata? TryReadChunk(ref int size)
    {
        if (_binaryReader.BaseStream.Position == _binaryReader.BaseStream.Length)
            return null;
        
        var chunk = ReadChunk();
        size += 8;
        return chunk;
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _binaryReader.Dispose();
    }
}