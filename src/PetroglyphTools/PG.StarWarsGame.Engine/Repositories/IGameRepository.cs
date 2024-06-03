using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Repositories;

public interface IGameRepository : IRepository
{
    public GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);

    IEnumerable<string> FindFiles(string searchPattern, bool megFileOnly = false);
}