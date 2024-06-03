using System.Collections.Generic;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabase : IGameDatabase
{
    public required IGameRepository GameRepository { get; init; }

    public required GameConstants GameConstants { get; init; }

    public required IList<GameObject> GameObjects { get; init; }
}