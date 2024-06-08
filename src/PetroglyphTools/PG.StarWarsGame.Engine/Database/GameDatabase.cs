using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabase : IGameDatabase
{
    public required IGameRepository GameRepository { get; init; }

    public required GameConstants GameConstants { get; init; }

    public required IXmlDatabase<GameObject> GameObjects { get; init; }

    public required IXmlDatabase<SfxEvent> SfxEvents { get; init; }
}