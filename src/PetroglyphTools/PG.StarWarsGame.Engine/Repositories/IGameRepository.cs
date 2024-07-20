﻿using System.Collections.Generic;
using PG.StarWarsGame.Engine.Language;

namespace PG.StarWarsGame.Engine.Repositories;

public interface IGameRepository : IRepository
{
    public GameEngineType EngineType { get; }

    IRepository EffectsRepository { get; }

    IRepository TextureRepository { get; }

    bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);

    IEnumerable<string> FindFiles(string searchPattern, bool megFileOnly = false);

    bool IsLanguageInstalled(LanguageType languageType);
}