using System.IO;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using PG.StarWarsGame.Files.TED.Data;

namespace PG.StarWarsGame.Files.TED.Binary.Reader;

internal sealed class TedFileReader(Stream stream) : ChunkFileReaderBase<IMapData>(stream), ITedFileReader
{
    public override IMapData Read()
    {
        throw new System.NotImplementedException();
    }
}