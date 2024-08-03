using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModVerify.CliApp.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace ModVerify.CliApp.ModSelectors;

internal abstract class ModSelectorBase : IModSelector
{
    protected readonly ILogger? Logger;
    protected readonly GameFinderService GameFinderService;
    protected readonly IServiceProvider ServiceProvider;

    protected ModSelectorBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        GameFinderService = new GameFinderService(serviceProvider);
    }

    public abstract GameLocations? Select(GameInstallationsSettings settings, out IPhysicalPlayableObject? targetObject,
        out GameEngineType? actualEngineType);

    protected GameLocations GetLocations(IPhysicalPlayableObject playableObject, GameFinderResult finderResult, IList<string> additionalFallbackPaths)
    {
        var fallbacks = GetFallbackPaths(finderResult, playableObject, additionalFallbackPaths);
        var modPaths = GetModPaths(playableObject);
        return new GameLocations(modPaths, playableObject.Game.Directory.FullName, fallbacks);
    }

    private static IList<string> GetFallbackPaths(GameFinderResult finderResult, IPlayableObject gameOrMod, IList<string> additionalFallbackPaths)
    {
        var coercedFallbackGame = finderResult.FallbackGame;
        if (gameOrMod.Equals(finderResult.FallbackGame))
            coercedFallbackGame = null;
        else if (gameOrMod.Game.Equals(finderResult.FallbackGame))
            coercedFallbackGame = null;

        return GetFallbackPaths(coercedFallbackGame?.Directory.FullName, additionalFallbackPaths);
    }


    protected static IList<string> GetFallbackPaths(string? fallbackGame, IList<string> additionalFallbackPaths)
    {
        var fallbacks = new List<string>();
        if (fallbackGame is not null)
            fallbacks.Add(fallbackGame);
        foreach (var fallback in additionalFallbackPaths)
            fallbacks.Add(fallback);

        return fallbacks;
    }


    private IList<string> GetModPaths(IPhysicalPlayableObject modOrGame)
    {
        if (modOrGame is not IMod mod)
            return Array.Empty<string>();

        var traverser = ServiceProvider.GetRequiredService<IModDependencyTraverser>();
        return traverser.Traverse(mod)
            .Select(x => x.Mod)
            .OfType<IPhysicalMod>().Select(x => x.Directory.FullName)
            .ToList();
    }

}