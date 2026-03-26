using System.IO;
using System.IO.Abstractions;
using PG.StarWarsGame.Files.TED.Files;

namespace PG.StarWarsGame.Files.TED.Services;

public interface ITedFileService
{
    void RemoveMapPreview(Stream tedStream, FileSystemStream destination, bool extract, out byte[]? previewImageBytes);

    ITedFile Load(string path);

    ITedFile Load(Stream stream);
}