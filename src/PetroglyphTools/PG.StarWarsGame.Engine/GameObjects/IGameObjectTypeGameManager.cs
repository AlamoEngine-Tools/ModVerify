using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.GameObjects;

public interface IGameObjectTypeGameManager : IGameManager<GameObject>
{
    // List represent XML load order
    IReadOnlyList<GameObject> GameObjects { get; }

    /// <summary>
    /// Retrieves the collection of model and particle names directly associated with the specified <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>
    /// Death Clones, Projectiles, Hardpoints, etc. are not included in the returned collection.
    /// </remarks>
    /// <param name="gameObject">
    /// The <see cref="GameObject"/> for which to retrieve the associated model names.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> containing the names of the models associated with the specified <see cref="GameObject"/>.
    /// </returns>
    IEnumerable<string> GetModels(GameObject gameObject);

    GameObject? FindObjectType(string name);
}