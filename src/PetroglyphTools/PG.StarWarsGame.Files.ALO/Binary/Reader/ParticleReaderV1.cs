using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PG.Commons.Binary;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Services;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

namespace PG.StarWarsGame.Files.ALO.Binary.Reader;

internal class ParticleReaderV1(AloLoadOptions loadOptions, Stream stream) : AloFileReader<AlamoParticle>(loadOptions, stream)
{
    public override AlamoParticle Read()
    {
        var textures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? name = null;

        var rootChunk = ChunkReader.ReadChunk();

        if (rootChunk.Type != (int)ChunkType.Particle)
            throw new NotSupportedException("This reader only support V1 particles.");

        var actualSize = 0;

        do
        {
            var chunk = ChunkReader.ReadChunk(ref actualSize);

            switch (chunk.Type)
            {
                case (int)ChunkType.Name:
                    ReadName(chunk.Size, out name);
                    break;
                case (int)ChunkType.Emitters:
                    ReadEmitters(chunk.Size, textures);
                    break;
                default:
                    ChunkReader.Skip(chunk.Size);
                    break;
            }

            actualSize += chunk.Size;

        } while (actualSize < rootChunk.Size);

       

        if (actualSize != rootChunk.Size)
            throw new BinaryCorruptedException();

        if (string.IsNullOrEmpty(name)) 
            throw new BinaryCorruptedException("The particle does not contain a name.");

        return new AlamoParticle
        {
            Name = name!,
            Textures = textures,
        };
    }

    private void ReadEmitters(int size, HashSet<string> textures)
    {
        var actualSize = 0;

        do
        {
            var chunk = ChunkReader.ReadChunk(ref actualSize);

            if (chunk.Type != (int)ChunkType.Emitter)
                throw new BinaryCorruptedException("Unable to read particle");

            ReadEmitter(chunk.Size, textures);

            actualSize += chunk.Size;


        } while (actualSize < size);

        if (size != actualSize)
            throw new BinaryCorruptedException("Unable to read particle.");
    }

    private void ReadEmitter(int chunkSize, HashSet<string> textures)
    {
        var actualSize = 0;

        do
        {
            var chunk = ChunkReader.ReadChunk(ref actualSize);

            if (chunk.Type == (int)ChunkType.Properties)
            {
                var shader = ChunkReader.ReadDword();
                ChunkReader.Skip(chunk.Size - sizeof(uint));
            }
            else if (chunk.Type == (int)ChunkType.ColorTextureName)
            {
                var texture = ChunkReader.ReadString(chunk.Size, Encoding.ASCII, true);
                textures.Add(texture);
            }
            else if (chunk.Type == (int)ChunkType.BumpTextureName)
            {
                var bump = ChunkReader.ReadString(chunk.Size, Encoding.ASCII, true);
                textures.Add(bump);
            }
            else
            {
                ChunkReader.Skip(chunk.Size);
            }

            actualSize += chunk.Size;

        } while (actualSize < chunkSize);

        if (actualSize != chunkSize)
            throw new BinaryCorruptedException("Unable to read particle");
    }

    private void ReadName(int size, out string name)
    {
        var actualSize = 0;
        name = ChunkReader.ReadString(size, Encoding.ASCII, true, ref actualSize);
        if (size != actualSize)
            throw new BinaryCorruptedException("Unable to read alo particle.");
    }
}