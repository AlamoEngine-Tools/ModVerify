using PG.Commons.Files;
using PG.StarWarsGame.Files.MTD.Data;
using PG.StarWarsGame.Files.MTD.Files;

namespace PG.StarWarsGame.Engine.Database.GameManagers;

internal class NotFoundMtdFile : IMtdFile
{
    public static readonly NotFoundMtdFile Instance = new();

    public MtdFileInformation FileInformation { get; } = new()
    {
        FilePath = string.Empty
    };

    PetroglyphFileInformation IPetroglyphFileHolder.FileInformation => FileInformation;
    public IMegaTextureDirectory Content => EmptyMegaTextureDirectory.Instance;
    object IPetroglyphFileHolder.Content => Content;
    public string FilePath => string.Empty;
    public string Directory => string.Empty;
    public string FileName => string.Empty;

    private NotFoundMtdFile()
    {

    }

    public void Dispose()
    {
    }
}