using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
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

namespace AET.ModVerify.App.TargetSelectors;

internal class AutomaticSelector(IServiceProvider serviceProvider) : VerificationTargetSelectorBase(serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    internal override SelectionResult SelectTarget(VerificationTargetSettings settings)
    {
        var targetPath = settings.TargetPath;
        if (targetPath is null)
            throw new InvalidOperationException("path to verify cannot be null.");

        var engine = settings.Engine;

        GameFinderResult finderResult;
        try
        {
            finderResult = GameFinderService.FindGamesFromPathOrGlobal(targetPath, GameFinderSettings.Default);
        }
        catch (GameNotFoundException)
        {
            Logger?.LogError(ModVerifyConstants.ConsoleEventId, "Unable to find games based of the specified target path '{Path}'. Consider specifying all paths manually.", settings.GamePath);
            throw;
        }


        GameLocations locations;

        var targetObject = GetAttachedModOrGame(finderResult, engine, targetPath);


        if (targetObject is null)
        {
            if (!settings.Engine.HasValue)
                throw new ArgumentException("Game engine not specified. Use --engine argument to set it.");

            Logger?.LogDebug("The requested mod at '{TargetPath}' is detached from its games.", targetPath);

            // The path is a detached mod, that exists on a different location than the game.
            locations = GetDetachedModLocations(targetPath, finderResult, settings, out var mod);
            targetObject = mod;
        }
        else
        {
            var actualType = targetObject.Game.Type.ToEngineType();
            engine ??= actualType;
            if (engine != actualType)
                throw new ArgumentException($"The specified game type '{engine}' does not match the actual type of the game or mod to verify.");
            locations = GetLocations(targetObject, finderResult.FallbackGame, settings.AdditionalFallbackPaths);
        }
        
        return new(locations, engine.Value, targetObject);
    }

    private IPhysicalPlayableObject? GetAttachedModOrGame(GameFinderResult finderResult, GameEngineType? requestedEngineType, string targetPath)
    {
        var targetFullPath = _fileSystem.Path.GetFullPath(targetPath);

        // If the target is the game directory itself.
        if (targetFullPath.Equals(finderResult.Game.Directory.FullName, StringComparison.OrdinalIgnoreCase))
        {
            if (!IsEngineTypeSupported(requestedEngineType, finderResult.Game))
                throw new ArgumentException($"The specified game type '{requestedEngineType}' does not match the actual type of the game '{targetPath}' to verify.");
            return finderResult.Game;
        }

        if (finderResult.FallbackGame is not null &&
            targetFullPath.Equals(finderResult.FallbackGame.Directory.FullName, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotImplementedException("When does this actually happen???");

            if (finderResult.FallbackGame.Type.ToEngineType() != requestedEngineType)
                throw new ArgumentException($"The specified game type '{requestedEngineType}' does not match the actual type of the game '{targetPath}' to verify.");
            return finderResult.FallbackGame;
        }

        return GetMatchingModFromGame(finderResult.Game, requestedEngineType, targetFullPath) ??
               GetMatchingModFromGame(finderResult.FallbackGame, requestedEngineType, targetFullPath);
    }

    private GameLocations GetDetachedModLocations(string modPath, GameFinderResult gameResult, VerificationTargetSettings settings, out IPhysicalMod mod)
    {
        IGame game = null!;

        if (gameResult.Game.Type.ToEngineType() == settings.Engine)
            game = gameResult.Game;
        if (gameResult.FallbackGame is not null && gameResult.FallbackGame.Type.ToEngineType() == settings.Engine)
            game = gameResult.FallbackGame;

        if (game is null)
            throw new GameNotFoundException($"Unable to find game of type '{settings.Engine}'");

        var modFinder = ServiceProvider.GetRequiredService<IModFinder>();
        var modRef = modFinder.FindMods(game, _fileSystem.DirectoryInfo.New(modPath)).FirstOrDefault();

        if (modRef is null)
            throw new NotSupportedException($"The mod at '{modPath}' is not compatible to the found game '{game}'.");

        var modFactory = ServiceProvider.GetRequiredService<IModFactory>();
        mod = modFactory.CreatePhysicalMod(game, modRef, CultureInfo.InvariantCulture);

        game.AddMod(mod);

        mod.ResolveDependencies();

        return GetLocations(mod, gameResult.FallbackGame, settings.AdditionalFallbackPaths);
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
                        throw new ArgumentException($"The specified engine type '{requestedEngineType}' does not match the required of the mod '{modPath}' to verify.");
                    return physicalMod;
                }
            }
        }

        return null;
    }
    
    private static bool IsEngineTypeSupported(GameEngineType? requestedEngineType, IPhysicalPlayableObject target)
    {
        return !requestedEngineType.HasValue || target.Game.Type.ToEngineType() == requestedEngineType;
    }
}