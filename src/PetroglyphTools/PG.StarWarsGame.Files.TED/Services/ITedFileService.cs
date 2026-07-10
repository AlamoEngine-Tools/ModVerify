using System.IO;
using System.IO.Abstractions;
using PG.StarWarsGame.Files.TED.Files;

namespace PG.StarWarsGame.Files.TED.Services;

public interface ITedFileService
{
    bool RemoveMapPreview(Stream tedStream, FileSystemStream destination, bool extract, out byte[]? previewImageBytes);

    bool ContainsPreviewImage(Stream tedStream);
    
    ITedFile Load(string path);

    ITedFile Load(Stream stream);
}