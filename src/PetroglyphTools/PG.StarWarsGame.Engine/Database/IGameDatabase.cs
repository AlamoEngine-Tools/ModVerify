using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabase
{
    public IGameRepository GameRepository { get; }

    public GameConstants GameConstants { get; }

    public IXmlDatabase<GameObject> GameObjects { get; }

    public IXmlDatabase<SfxEvent> SfxEvents { get; }
}