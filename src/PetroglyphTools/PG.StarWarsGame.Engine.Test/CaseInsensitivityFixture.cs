using System;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Testing;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Represents a test fixture for verifying that repositories resolve requests in a case-insensitive manner.
/// </summary>
/// <param name="PopulateGame">Callback that writes fixtures into the virtual game's <c>ConfigureGame</c> origin.</param>
/// <param name="SelectRepository">Picks the repository under test from the constructed <see cref="IGameRepository"/>.</param>
/// <param name="FilesystemLookup">Lookup key the repository should resolve to the filesystem-backed fixture.</param>
/// <param name="FilesystemContent">Expected content at <paramref name="FilesystemLookup"/>.</param>
/// <param name="MegLookup">Lookup key the repository should resolve to the MEG-backed fixture.</param>
/// <param name="MegContent">Expected content at <paramref name="MegLookup"/>.</param>
public sealed record CaseInsensitivityFixture(
    Action<IRepoOriginWriter> PopulateGame,
    Func<IGameRepository, IRepository> SelectRepository,
    string FilesystemLookup,
    string FilesystemContent,
    string MegLookup,
    string MegContent);
