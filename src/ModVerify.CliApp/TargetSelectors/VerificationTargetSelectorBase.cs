using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace AET.ModVerify.App.TargetSelectors;

internal abstract class VerificationTargetSelectorBase : IVerificationTargetSelector
{
    internal sealed record SelectionResult(
        GameLocations Locations,
        GameEngineType Engine,
        IPhysicalPlayableObject? Target);
    
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

    public VerificationTarget Select(VerificationTargetSettings settings)
    {
        var selectedTarget = SelectTarget(settings);

        return new VerificationTarget
        {
            Location = selectedTarget.Locations,
            Engine = selectedTarget.Engine,
            Name = GetTargetName(selectedTarget.Target, selectedTarget.Locations),
            Version = GetTargetVersion(selectedTarget.Target)
        };
    }


    internal abstract SelectionResult SelectTarget(VerificationTargetSettings settings);
    
    
    protected GameLocations GetLocations(
        IPhysicalPlayableObject target,
        IGame? fallbackGame,
        IReadOnlyList<string> additionalFallbackPaths)
    {
        var fallbacks = GetFallbackPaths(target, fallbackGame, additionalFallbackPaths);
        var modPaths = GetModPaths(target);
        return new GameLocations(modPaths, target.Game.Directory.FullName, fallbacks);
    }

    private static IReadOnlyList<string> GetFallbackPaths(IPhysicalPlayableObject target, IGame? fallbackGame, IReadOnlyList<string> additionalFallbackPaths)
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

    protected static string GetTargetName(IPhysicalPlayableObject? targetObject, GameLocations gameLocations)
    {
        if (targetObject is not null)
            return targetObject.Name;

        // TODO: Reuse name beautifier from GameInfrastructure lib
        var mod = gameLocations.ModPaths.FirstOrDefault();
        return mod ?? gameLocations.GamePath;
    }

    protected string? GetTargetVersion(IPhysicalPlayableObject? targetObject)
    {
        return targetObject switch
        {
            IMod mod => mod.Version?.ToString(),
            IGame game => GetVersionFromGame(game),
            _ => null
        };
    }

    private string? GetVersionFromGame(IGame game)
    { 
        var exeFile = GameExecutableFileUtilities.GetExecutableForGame(game, GameBuildType.Release);
        if (exeFile is null)
        {
            Logger?.LogWarning(ModVerifyConstants.ConsoleEventId, 
                "Unable to get game version of target path '{Path}'. Is this a game directory?",
                game.Directory.FullName);
            return null;
        }

        var versionInfo = FileSystem.FileVersionInfo.GetVersionInfo(exeFile.FullName);
        var version =
            $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
        return version;
    }
}