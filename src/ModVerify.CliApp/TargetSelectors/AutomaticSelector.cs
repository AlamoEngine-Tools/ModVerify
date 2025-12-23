using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;

namespace AET.ModVerify.App.TargetSelectors;

internal class AutomaticSelector(IServiceProvider serviceProvider) : VerificationTargetSelectorBase(serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    internal override SelectionResult SelectTarget(VerificationTargetSettings settings)
    {
        if (!settings.UseAutoDetection)
            throw new ArgumentException("wrong settings format provided.", nameof(settings));
        
        var targetPath = settings.TargetPath;
        if (!_fileSystem.Directory.Exists(targetPath))
        {
            Logger?.LogError(ModVerifyConstants.ConsoleEventId, "The specified path '{Path}' does not exist.", targetPath);
            throw new TargetNotFoundException(targetPath);
        }

        var engine = settings.Engine;

        GameFinderResult finderResult;
        try
        {
            var finderSettings = new GameFinderSettings
            {
                Engine = engine,
                InitMods = true,
                SearchFallbackGame = true
            };
            finderResult = GameFinderService.FindGamesFromPathOrGlobal(targetPath, finderSettings);
        }
        catch (GameNotFoundException)
        {
            Logger?.LogError(ModVerifyConstants.ConsoleEventId, 
                "Unable to find games based of the specified target path '{Path}'. Consider specifying all paths manually.", targetPath);
            throw;
        }

        // In a Steam scenario, there is a chance that the user specified a FoC targetPath,
        // but requested EaW engine. This does not make sense, and we need to check against this.
        if (finderResult.Game.Platform == GamePlatform.SteamGold && engine is GameEngineType.Eaw)
        {
            var targetIsFoc = GameFinderService.TryFindGame(targetPath,
                new GameFinderSettings { Engine = GameEngineType.Foc, InitMods = false, SearchFallbackGame = false },
                out _);
            if (targetIsFoc)
                ThrowEngineNotSupported(engine.Value, targetPath);
        }

        if (engine.HasValue)
        {
            if (finderResult.Game.Type.ToEngineType() != engine.Value)
            {
                if (finderResult.FallbackGame?.Type.ToEngineType() != engine)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        

        GameLocations locations;

        var targetObject = GetAttachedModOrGame(finderResult, targetPath, engine);

        if (targetObject is not null)
        {
            var actualType = targetObject.Game.Type;
            Debug.Assert(IsEngineTypeSupported(engine, actualType));
            engine ??= actualType.ToEngineType();
            locations = GetLocations(targetObject, finderResult.FallbackGame, settings.AdditionalFallbackPaths);
        }
        else
        {
            if (!engine.HasValue)
                throw new ArgumentException("Game engine not specified. Use --engine argument to set it.");

            Logger?.LogDebug("The requested mod at '{TargetPath}' is detached from its games.", targetPath);

            // The path is a detached mod, that exists on a different location than the game.
            locations = GetDetachedModLocations(targetPath, finderResult, engine.Value, settings.AdditionalFallbackPaths, out var mod);
            targetObject = mod;
        }

        return new(locations, engine.Value, targetObject);
    }

    private IPhysicalPlayableObject? GetAttachedModOrGame(GameFinderResult finderResult, string targetPath, GameEngineType? requestedEngineType)
    {
        var targetFullPath = _fileSystem.Path.GetFullPath(targetPath);

        IPhysicalPlayableObject? target = null;
        
        // If the target is the game directory itself.
        if (targetFullPath.Equals(finderResult.Game.Directory.FullName, StringComparison.OrdinalIgnoreCase)) 
            target = finderResult.Game;
        else if (finderResult.FallbackGame is not null && targetFullPath.Equals(finderResult.FallbackGame.Directory.FullName, StringComparison.OrdinalIgnoreCase))
        {
            // The game detection identified both Foc and Eaw because either Steam is installed or requestedEngineType was not specified.
            // The requested path points to a EaW installation.
            Debug.Assert(finderResult.FallbackGame.Type is GameType.Eaw);
            target = finderResult.FallbackGame;
        }

        target ??= GetMatchingModFromGame(finderResult.Game, targetFullPath, requestedEngineType) ??
                   GetMatchingModFromGame(finderResult.FallbackGame, targetFullPath, requestedEngineType);


        if (target is not null)
        {
            if (!IsEngineTypeSupported(requestedEngineType, target.Game.Type))
                ThrowEngineNotSupported(requestedEngineType.Value, targetPath);
        }

        return target;
    }

    private GameLocations GetDetachedModLocations(
        string modPath, 
        GameFinderResult gameResult,
        GameEngineType requestedGameEngine,
        IReadOnlyList<string> additionalFallbackPaths,
        out IPhysicalMod mod)
    {
        var game = GetTargetGame(gameResult, requestedGameEngine);
        Debug.Assert(game is not null);

        var modFinder = ServiceProvider.GetRequiredService<IModFinder>();
        var modRef = modFinder.FindMods(game, _fileSystem.DirectoryInfo.New(modPath)).FirstOrDefault();

        if (modRef is null)
            ThrowEngineNotSupported(requestedGameEngine, modPath);

        var modFactory = ServiceProvider.GetRequiredService<IModFactory>();
        mod = modFactory.CreatePhysicalMod(game, modRef, CultureInfo.InvariantCulture);

        game.AddMod(mod);

        mod.ResolveDependencies();

        return GetLocations(mod, gameResult.FallbackGame, additionalFallbackPaths);
    }

    private static IPhysicalMod? GetMatchingModFromGame(IGame? game, string modPath, GameEngineType? requestedEngineType)
    {
        if (game is null || !IsEngineTypeSupported(requestedEngineType, game.Type))
            return null;

        foreach (var mod in game.Game.Mods)
        {
            if (mod is not IPhysicalMod physicalMod) 
                continue;
            
            if (physicalMod.Directory.FullName.Equals(modPath, StringComparison.OrdinalIgnoreCase))
                return physicalMod;
        }

        return null;
    }

    private static IGame? GetTargetGame(GameFinderResult finderResult, GameEngineType? requestedEngine)
    {
        if (!requestedEngine.HasValue)
            return null;
        if (finderResult.Game.Type.ToEngineType() == requestedEngine)
            return finderResult.Game;
        if (finderResult.FallbackGame is not null && finderResult.FallbackGame.Type.ToEngineType() == requestedEngine)
            return finderResult.FallbackGame;
        return null;
    }

    private static bool IsEngineTypeSupported([NotNullWhen(false)] GameEngineType? requestedEngineType, GameType actualGameType)
    {
        return !requestedEngineType.HasValue || actualGameType.ToEngineType() == requestedEngineType;
    }

    [DoesNotReturn]
    private static void ThrowEngineNotSupported(GameEngineType requested, string targetPath)
    {
        throw new ArgumentException($"The specified game engine '{requested}' does not match engine of the verification target '{targetPath}'.");
    }
}