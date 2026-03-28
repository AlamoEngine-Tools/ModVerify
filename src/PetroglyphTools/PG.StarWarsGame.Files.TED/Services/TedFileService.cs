using PG.Commons.Services;
using PG.Commons.Utilities;
using PG.StarWarsGame.Files.TED.Files;
using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.TED.Binary.Reader;
using PG.StarWarsGame.Files.TED.Binary.Writer;

namespace PG.StarWarsGame.Files.TED.Services;

internal class TedFileService(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), ITedFileService
{
    public bool RemoveMapPreview(Stream tedStream, FileSystemStream destination, bool extract, out byte[]? previewImageBytes)
    {
        if (tedStream == null) 
            throw new ArgumentNullException(nameof(tedStream));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        var previewExtractor = new MapPreviewExtractor();
        return previewExtractor.ExtractPreview(tedStream, destination, extract, out previewImageBytes);
    }

    public bool ContainsPreviewImage(Stream tedStream)
    {
        if (tedStream == null) 
            throw new ArgumentNullException(nameof(tedStream));
        var previewExtractor = new MapPreviewExtractor();
        return previewExtractor.ContainsPreviewPicture(tedStream);
    }

    public ITedFile Load(string path)
    {
        using var fileStream = FileSystem.FileStream.New(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Load(fileStream);
    }

    public ITedFile Load(Stream stream)
    {
        if (stream == null) 
            throw new ArgumentNullException(nameof(stream));

        using var reader = Services.GetRequiredService<ITedFileReaderFactory>().GetReader(stream);
        var map = reader.Read();

        var filePath = stream.GetFilePath(out var isInMeg);
        var fileInfo = new TedFileInformation(filePath, isInMeg);

        return new TedFile(map, fileInfo, Services);
    }
}