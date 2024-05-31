namespace PG.StarWarsGame.Engine.FileSystem;

public interface IGameRepository : IRepository
{
    public GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);
}