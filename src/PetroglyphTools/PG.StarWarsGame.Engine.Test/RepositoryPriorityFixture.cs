using System;
using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// A test fixture for to test the loading chain for inter-origin asset loading.
/// </summary>
/// <remarks>
/// <paramref name="ResolvablePath"/> should be chosen in such a way that a repository does
/// not apply some special resolution logic to it, e.g. by being located at the root of the virtual game.
/// </remarks>
/// <param name="SelectRepository">Picks the repository facet under test from the constructed <see cref="IGameRepository"/>.</param>
/// <param name="ResolvablePath">
/// Relative path written into an origin.
/// </param>
public sealed record RepositoryPriorityFixture(
    Func<IGameRepository, IRepository> SelectRepository,
    string ResolvablePath);