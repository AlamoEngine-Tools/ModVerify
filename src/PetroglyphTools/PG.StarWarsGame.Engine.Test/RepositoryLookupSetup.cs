using System;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Testing;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Per-repository scaffolding for the inherited lookup-insensitivity test.
/// Each derived test class returns one describing which <see cref="IRepository"/> facet it exercises,
/// how to populate the virtual game with fixtures that facet can resolve, and the lookup keys that
/// should return the seeded filesystem-backed and MEG-backed content.
/// </summary>
/// <param name="PopulateGame">Callback that writes fixtures into the virtual game's <c>WithGame</c> origin.</param>
/// <param name="SelectRepository">Picks the repository facet under test from the constructed <see cref="IGameRepository"/>.</param>
/// <param name="FilesystemLookup">Lookup key the repository should resolve to the filesystem-backed fixture.</param>
/// <param name="FilesystemContent">Expected content at <paramref name="FilesystemLookup"/>.</param>
/// <param name="MegLookup">Lookup key the repository should resolve to the MEG-backed fixture.</param>
/// <param name="MegContent">Expected content at <paramref name="MegLookup"/>.</param>
public sealed record RepositoryLookupSetup(
    Action<IRepoOriginWriter> PopulateGame,
    Func<IGameRepository, IRepository> SelectRepository,
    string FilesystemLookup,
    string FilesystemContent,
    string MegLookup,
    string MegContent);
