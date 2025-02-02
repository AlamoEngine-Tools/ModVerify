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

        // Using default encoding here is OK as we don't read strings using the .NET methods.
        _binaryReader = new PetroglyphBinaryReader(stream, leaveOpen);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _binaryReader.Dispose();
    }

    public ChunkMetadata ReadChunk()
    {
        var type = _binaryReader.ReadInt32();
        var rawSize = _binaryReader.ReadInt32();

        var isContainer = (rawSize & 0x80000000) != 0;
        var size = rawSize & 0x7FFFFFFF;

        return isContainer ? ChunkMetadata.FromContainer(type, size) : ChunkMetadata.FromData(type, size);
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

        return ChunkMetadata.FromData(type, size, true);
    }

    public uint ReadDword(ref int readSize)
    {
        var value = _binaryReader.ReadUInt32();
        readSize += sizeof(uint);
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
        var value = _binaryReader.ReadString(encoding, size, zeroTerminated);
        readSize += size;
        return value;
    }

    public string ReadString(int size, Encoding encoding, bool zeroTerminated)
    {
        var value = _binaryReader.ReadString(encoding, size, zeroTerminated);
        return value;
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
        try
        {
            var chunk = ReadChunk();
            size += 8;
            return chunk;
        }
        catch (EndOfStreamException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}