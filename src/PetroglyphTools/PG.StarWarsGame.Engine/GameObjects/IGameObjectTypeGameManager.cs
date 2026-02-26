using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.GameObjects;

public interface IGameObjectTypeGameManager : IGameManager<GameObject>
{
    // List represent XML load order
    IReadOnlyList<GameObject> GameObjects { get; }
}