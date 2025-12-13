using AnakinRaW.CommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PG.StarWarsGame.Engine;

public sealed class GameLocations
{
    /// <summary>
    /// Gets the path that represents the topmost playable target. This is typically the actual mod selected by the user.
    /// </summary>
    public string TargetPath { get; }

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

        TargetPath = ModPaths.Count > 0
            ? ModPaths[0]
            : GamePath;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("GameLocation=[");
        if (ModPaths.Count > 0)
            sb.AppendLine($"Mods=[{string.Join(";", ModPaths)}];");
        sb.AppendLine($"Game=[{GamePath}];");
        if (FallbackPaths.Count > 0)
            sb.AppendLine($"Fallbacks=[{string.Join(";", FallbackPaths)}];");
        sb.AppendLine("]");

        return sb.ToString();
    }
}