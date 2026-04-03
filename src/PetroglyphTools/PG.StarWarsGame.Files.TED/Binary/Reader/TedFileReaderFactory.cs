using System;
using System.IO;

namespace PG.StarWarsGame.Files.TED.Binary.Reader;

internal class TedFileReaderFactory(IServiceProvider serviceProvider) : ITedFileReaderFactory
{
    public ITedFileReader GetReader(Stream dataStream)
    {
        return new TedFileReader(TedLoadOptions.Full, dataStream);
    }
}