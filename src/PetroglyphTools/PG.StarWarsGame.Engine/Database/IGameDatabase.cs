using System.Collections.Generic;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabase
{
    IGameRepository GameRepository { get; }

    GameConstants GameConstants { get; }

    IXmlDatabase<GameObject> GameObjects { get; }

    IXmlDatabase<SfxEvent> SfxEvents { get; }

    IEnumerable<LanguageType> InstalledLanguages { get; }
}