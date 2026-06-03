using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.IO;

public interface IGameRepository : IRepository
{
    /// <summary>
    /// Gets the fully qualified path of the repository's top-most root — the first mod directory when mods
    /// are configured, otherwise the base game directory — with a trailing directory separator.
    /// </summary>
    string Path { get; }
    
    // ReSharper disable once InconsistentNaming
    PetroglyphFileSystem PGFileSystem { get; }

    GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    IRepository TextureRepository { get; }

    IRepository ModelRepository { get; }

    bool IsLanguageInstalled(LanguageType languageType);
}