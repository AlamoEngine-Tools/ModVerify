using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.IO.Repositories;

public interface IGameRepository : IRepository
{
    /// <summary>
    /// Gets the full qualified path of this repository with a trailing directory separator
    /// </summary>
    public string Path { get; }

    public GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    IRepository TextureRepository { get; }

    bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);

    bool IsLanguageInstalled(LanguageType languageType);
}