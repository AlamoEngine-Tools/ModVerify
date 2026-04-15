using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.IO;

public interface IGameRepository : IRepository
{
    /// <summary>
    /// Gets the full qualified path of this repository with a trailing directory separator
    /// </summary>
    string Path { get; }
    
    // ReSharper disable once InconsistentNaming
    PetroglyphFileSystem PGFileSystem { get; }

    GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    IRepository TextureRepository { get; }

    IRepository ModelRepository { get; }

    bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);

    bool IsLanguageInstalled(LanguageType languageType);
}