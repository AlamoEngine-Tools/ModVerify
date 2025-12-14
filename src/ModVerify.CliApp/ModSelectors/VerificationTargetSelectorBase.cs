using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace AET.ModVerify.App.ModSelectors;

internal abstract class VerificationTargetSelectorBase : IVerificationTargetSelector
{
    protected readonly ILogger? Logger;
    protected readonly GameFinderService GameFinderService;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IFileSystem FileSystem;

    protected VerificationTargetSelectorBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        GameFinderService = new GameFinderService(serviceProvider);
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public abstract VerificationTarget Select(GameInstallationsSettings settings);
    
    protected GameLocations GetLocations(
        IPhysicalPlayableObject target,
        IGame? fallbackGame,
        IReadOnlyList<string> additionalFallbackPaths)
    {
        var fallbacks = GetFallbackPaths(target, fallbackGame, additionalFallbackPaths);
        var modPaths = GetModPaths(target);
        return new GameLocations(modPaths, target.Game.Directory.FullName, fallbacks);
    }

    private static IReadOnlyList<string> GetFallbackPaths(IPlayableObject target, IGame? fallbackGame, IReadOnlyList<string> additionalFallbackPaths)
    {
        var coercedFallbackGame = fallbackGame;
        if (target is IGame tGame && tGame.Equals(fallbackGame))
            coercedFallbackGame = null;
        else if (target.Game.Equals(fallbackGame))
            coercedFallbackGame = null;
        return GetFallbackPaths(coercedFallbackGame?.Directory.FullName, additionalFallbackPaths);
    }


    protected static IReadOnlyList<string> GetFallbackPaths(string? fallbackGame, IReadOnlyList<string> additionalFallbackPaths)
    {
        var fallbacks = new List<string>();
        if (fallbackGame is not null)
            fallbacks.Add(fallbackGame);
        foreach (var fallback in additionalFallbackPaths)
            fallbacks.Add(fallback);
        return fallbacks;
    }


    private IReadOnlyList<string> GetModPaths(IPhysicalPlayableObject modOrGame)
    {
        if (modOrGame is not IMod mod)
            return [];

        var traverser = ServiceProvider.GetRequiredService<IModDependencyTraverser>();
        return traverser.Traverse(mod)
            .OfType<IPhysicalMod>().Select(x => x.Directory.FullName)
            .ToList();
    }

    protected static string GetTargetName(IPlayableObject? targetObject, GameLocations gameLocations)
    {
        if (targetObject is not null)
            return targetObject.Name;

        // TODO: Reuse name beautifier from GameInfrastructure lib
        var mod = gameLocations.ModPaths.FirstOrDefault();
        return mod ?? gameLocations.GamePath;
    }

    protected static string? GetTargetVersion(IPlayableObject? targetObject)
    {
        if (targetObject is null)
            return null;
        
        // TODO: Implement version retrieval from targetObject if possible
        return null;
    }

}