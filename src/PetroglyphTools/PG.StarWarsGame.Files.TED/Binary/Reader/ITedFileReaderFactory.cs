using System.IO;

namespace PG.StarWarsGame.Files.TED.Binary.Reader;

internal interface ITedFileReaderFactory
{
    ITedFileReader GetReader(Stream dataStream);
}