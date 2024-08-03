using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModVerify.CliApp.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace ModVerify.CliApp.ModSelectors;

internal class AutomaticModSelector(IServiceProvider serviceProvider) : ModSelectorBase(serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public override GameLocations? Select(GameInstallationsSettings settings, out IPhysicalPlayableObject? targetObject, out GameEngineType? actualEngineType)
    {
        var pathToVerify = settings.AutoPath;
        Debug.Assert(pathToVerify is not null);

        actualEngineType = settings.EngineType;

        GameFinderResult finderResult;
        try
        {
            finderResult = GameFinderService.FindGamesFromPathOrGlobal(pathToVerify);
        }
        catch (GameNotFoundException)
        {
            Logger?.LogError($"Unable to find games based of the given location '{settings.GamePath}'. Consider specifying all paths manually.");
            targetObject = null!;
            return null;
        }


        var modOrGame = GetAttachedModOrGame(finderResult, actualEngineType, pathToVerify);

        if (modOrGame is not null)
        {
            var actualType = modOrGame.Game.Type.ToEngineType();
            actualEngineType ??= actualType;
            if (actualEngineType != actualType)
                throw new ArgumentException($"The specified game type '{actualEngineType}' does not match the actual type of the game or mod to verify.");

            targetObject = modOrGame;
            return GetLocations(targetObject, finderResult, settings.AdditionalFallbackPaths);
        }

        if (!settings.EngineType.HasValue)
            throw new ArgumentException("Unable to determine game type. Use --type argument to set the game type.");

        Logger?.LogDebug($"The requested mod at '{pathToVerify}' is detached from its games.");

        // The path is a detached mod, that exists on a different location than the game.
        var result = GetDetachedModLocations(pathToVerify, finderResult, settings, out var mod);
        targetObject = mod;
        return result;
    }

    private IPhysicalPlayableObject? GetAttachedModOrGame(GameFinderResult finderResult, GameEngineType? requestedEngineType, string searchPath)
    {
        var fullSearchPath = _fileSystem.Path.GetFullPath(searchPath);

        if (finderResult.Game.Directory.FullName.Equals(fullSearchPath, StringComparison.OrdinalIgnoreCase))
        {
            if (finderResult.Game.Type.ToEngineType() != requestedEngineType)
                throw new ArgumentException($"The specified game type '{requestedEngineType}' does not match the actual type of the game '{searchPath}' to verify.");
            return finderResult.Game;
        }

        if (finderResult.FallbackGame is not null &&
            finderResult.FallbackGame.Directory.FullName.Equals(fullSearchPath, StringComparison.OrdinalIgnoreCase))
        {
            if (finderResult.FallbackGame.Type.ToEngineType() != requestedEngineType)
                throw new ArgumentException($"The specified game type '{requestedEngineType}' does not match the actual type of the game '{searchPath}' to verify.");
            return finderResult.FallbackGame;
        }

        return GetMatchingModFromGame(finderResult.Game, requestedEngineType, fullSearchPath) ??
               GetMatchingModFromGame(finderResult.FallbackGame, requestedEngineType, fullSearchPath);
    }

    private GameLocations GetDetachedModLocations(string modPath, GameFinderResult gameResult, GameInstallationsSettings settings, out IPhysicalMod mod)
    {
        IGame game = null!;

        if (gameResult.Game.Type.ToEngineType() == settings.EngineType)
            game = gameResult.Game;
        if (gameResult.FallbackGame is not null && gameResult.FallbackGame.Type.ToEngineType() == settings.EngineType)
            game = gameResult.FallbackGame;

        if (game is null)
            throw new GameNotFoundException($"Unable to find game of type '{settings.EngineType}'");

        var modFactory = ServiceProvider.GetRequiredService<IModFactory>();
        mod = modFactory.FromReference(game, new ModReference(modPath, ModType.Default), true, CultureInfo.InvariantCulture);

        game.AddMod(mod);

        var resolver = ServiceProvider.GetRequiredService<IDependencyResolver>();
        mod.ResolveDependencies(resolver,
            new DependencyResolverOptions { CheckForCycle = true, ResolveCompleteChain = true });

        return GetLocations(mod, gameResult, settings.AdditionalFallbackPaths);
    }

    private static IPhysicalMod? GetMatchingModFromGame(IGame? game, GameEngineType? requestedEngineType, string modPath)
    {
        if (game is null)
            return null;

        var isGameSupported = !requestedEngineType.HasValue || game.Type.ToEngineType() == requestedEngineType;
        foreach (var mod in game.Game.Mods)
        {
            if (mod is IPhysicalMod physicalMod)
            {
                if (physicalMod.Directory.FullName.Equals(modPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!isGameSupported)
                        throw new ArgumentException($"The specified game type '{requestedEngineType}' does not match the actual type of the mod '{modPath}' to verify.");
                    return physicalMod;
                }
            }
        }

        return null;
    }
}