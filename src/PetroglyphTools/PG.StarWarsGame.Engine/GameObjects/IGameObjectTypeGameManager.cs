using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.GameObjects;

public interface IGameObjectTypeGameManager : IGameManager<GameObjectType>
{
    // List represent XML load order
    IReadOnlyList<GameObjectType> GameObjectTypes { get; }

    GameObjectType? FindObjectType(string name);
}