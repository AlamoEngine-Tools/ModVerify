using System.Collections.Generic;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.FileSystem;

namespace PG.StarWarsGame.Engine;

public class GameDatabase
{
    public required IGameRepository GameRepository { get; init; }

    public required GameConstants GameConstants { get; init; }

    public required IList<GameObject> GameObjects { get; init; }
}