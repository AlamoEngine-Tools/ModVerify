using System.Collections.Generic;
using PG.StarWarsGame.Engine.DataTypes;

namespace PG.StarWarsGame.Engine;

public class GameDatabase
{
    public required GameConstants GameConstants { get; init; }

    public required IList<GameObject> GameObjects { get; init; }
}