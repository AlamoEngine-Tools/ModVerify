using System;
using System.Globalization;
using System.Linq;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace AET.ModVerify.App.ModSelectors;

internal class ManualSelector(IServiceProvider serviceProvider) : VerificationTargetSelectorBase(serviceProvider)
{
    public override VerificationTarget Select(GameInstallationsSettings settings)
    {
        if (string.IsNullOrEmpty(settings.GamePath))
            throw new ArgumentException("Argument --game must be set.");
        if (!settings.Engine.HasValue)
            throw new ArgumentException("Unable to determine game type. Use --engine argument to set the game type.");

        var engine = settings.Engine.Value;

        var gameLocations =  new GameLocations(
            settings.ModPaths.ToList(),
            settings.GamePath!,
            GetFallbackPaths(settings.FallbackGamePath, settings.AdditionalFallbackPaths).ToList());


        IPlayableObject? target = null;

        // For the manual selector the whole game and mod detection is optional.
        // This allows user to use the application for unusual scenarios,
        // not known to the detection service.
        try
        {
            var game = GameFinderService.FindGame(gameLocations.GamePath, new GameFinderSettings
            {
                Engine = engine,
                InitMods = false,
                SearchFallbackGame = false
            });
            target = TryGetPlayableObject(game, gameLocations.ModPaths.FirstOrDefault());
        }
        catch (GameNotFoundException e)
        {
            // TODO: Log
        }

        // If the fallback game path is specified we simply try to detect the game and report a warning to the user if not found.
        var fallbackGamePath = settings.FallbackGamePath;
        if (!string.IsNullOrEmpty(fallbackGamePath))
        {
            try
            { 
                GameFinderService.FindGame(fallbackGamePath, new GameFinderSettings
                {
                    InitMods = false,
                    SearchFallbackGame = false
                });
            }
            catch (GameNotFoundException e)
            {
                // TODO: Log
            }
        }
        
        return new VerificationTarget
        {
            Engine = engine,
            Location = gameLocations,
            Name = GetTargetName(target, gameLocations),
            Version = GetTargetVersion(target)
        };
    }
    
    private IPlayableObject TryGetPlayableObject(IGame game, string? modPath)
    {
        if (string.IsNullOrEmpty(modPath))
            return game;
        
        var modFinder = ServiceProvider.GetRequiredService<IModFinder>();
        var modFactory = ServiceProvider.GetRequiredService<IModFactory>();
        
        var mods = modFinder.FindMods(game, FileSystem.DirectoryInfo.New(modPath));
        var mod = modFactory.CreatePhysicalMod(game, mods.First(), CultureInfo.InvariantCulture);
        return mod;
    }
}