using System;
using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Represents a test fixture for verifying that repositories resolve simple file requests.
/// </summary>
/// <param name="SelectRepository">Picks the repository under test from the constructed <see cref="IGameRepository"/>.</param>
/// <param name="ResolvablePath">Lookup key the repository should resolve to the filesystem-backed fixture.</param>
public sealed record RepositoryFixture(
    Func<IGameRepository, IRepository> SelectRepository, 
    string ResolvablePath);
