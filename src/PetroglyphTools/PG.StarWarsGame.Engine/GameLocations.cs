using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Engine;

public sealed class GameLocations
{
    public IReadOnlyList<string> ModPaths { get; }

    public string GamePath { get; }

    public IReadOnlyList<string> FallbackPaths { get; }

    public GameLocations(string gamePath, string fallbackGamePath) : this(Array.Empty<string>(), gamePath, fallbackGamePath)
    {
    }

    public GameLocations(string modPath, string gamePath, string fallbackGamePath) : this([modPath], gamePath, fallbackGamePath)
    {
        ThrowHelper.ThrowIfNullOrEmpty(modPath);
    }

    public GameLocations(IList<string> modPaths, string gamePath, string fallbackGamePath) : this(modPaths,
        gamePath, [fallbackGamePath])
    {
        ThrowHelper.ThrowIfNullOrEmpty(fallbackGamePath);
    }

    public GameLocations(IList<string> modPaths, string gamePath, IList<string> fallbackPaths)
    {
        if (modPaths == null)
            throw new ArgumentNullException(nameof(modPaths));
        if (fallbackPaths == null)
            throw new ArgumentNullException(nameof(fallbackPaths));

        ModPaths = modPaths.ToList();
        GamePath = gamePath;
        FallbackPaths = fallbackPaths.ToList();
    }
}