using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace ModVerify.CliApp;

internal class ModOrGameSelector(IServiceProvider serviceProvider)
{
    private readonly GameFinderService _gameFinderService = new(serviceProvider);
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public ModSelectionResult SelectModOrGame(string? searchPath)
    {
        if (string.IsNullOrEmpty(searchPath))
            return SelectFromConsoleInput();
        return SelectFromPath(searchPath!);
    }

    private ModSelectionResult SelectFromPath(string searchPath)
    {
        var fullSearchPath = _fileSystem.Path.GetFullPath(searchPath);
        var gameResult = _gameFinderService.FindGamesFromPath(fullSearchPath);
        
        IPhysicalPlayableObject? gameOrMod = null;

        if (gameResult.Game.Directory.FullName.Equals(fullSearchPath, StringComparison.OrdinalIgnoreCase))
            gameOrMod = gameResult.Game;
        else
        {
            foreach (var mod in gameResult.Game.Mods)
            {
                if (mod is IPhysicalMod physicalMod)
                {
                    if (physicalMod.Directory.FullName.Equals(fullSearchPath, StringComparison.OrdinalIgnoreCase))
                    {
                        gameOrMod = physicalMod;
                        break;
                    }
                }
            }
        }

        if (gameOrMod is null)
            throw new GameException($"Unable to a game or mod matching the path '{fullSearchPath}'.");

        return new ModSelectionResult
        {
            FallbackGame = gameResult.FallbackGame,
            ModOrGame = gameOrMod
        };
    }

    private ModSelectionResult SelectFromConsoleInput()
    {
        var gameResult = _gameFinderService.FindGames();
        
        var list = new List<IPlayableObject>();
        
        var game = gameResult.Game;
        list.Add(gameResult.Game);

        Console.WriteLine($"0: {game.Name}");
        
        var counter = 1;

        var workshopMods = new HashSet<IMod>(new ModEqualityComparer(false, false));

        foreach (var mod in game.Mods)
        {
            var isSteam = mod.Type == ModType.Workshops;
            if (isSteam)
            {
                workshopMods.Add(mod);
                continue;
            }
            Console.WriteLine($"{counter++}:\t{mod.Name}");
            list.Add(mod);
        }

        if (gameResult.FallbackGame is not null)
        {
            var fallbackGame = gameResult.FallbackGame;
            list.Add(fallbackGame);
            Console.WriteLine($"{counter++}: {fallbackGame.Name}");

            foreach (var mod in fallbackGame.Mods)
            {
                var isSteam = mod.Type == ModType.Workshops;
                if (isSteam)
                {
                    workshopMods.Add(mod);
                    continue;
                }
                Console.WriteLine($"{counter++}:\t{mod.Name}");
                list.Add(mod);
            }
        }

        Console.WriteLine("Workshop Items:");
        foreach (var mod in workshopMods)
        {
            Console.WriteLine($"{counter++}:\t{mod.Name}");
            list.Add(mod);
        }

        
        IPlayableObject? selectedObject = null;

        do
        {
            Console.Write("Select a game or mod to verify: ");
            var numberString = Console.ReadLine();

            if (!int.TryParse(numberString, out var number)) 
                continue;
            if (number < list.Count)
                selectedObject = list[number];
        } while (selectedObject is null);

        if (selectedObject is not IPhysicalPlayableObject physicalPlayableObject)
            throw new InvalidOperationException();

        var coercedFallbackGame = gameResult.FallbackGame;
        if (selectedObject.Equals(gameResult.FallbackGame))
            coercedFallbackGame = null;
        else if (selectedObject.Game.Equals(gameResult.FallbackGame))
            coercedFallbackGame = null;

        
        return new ModSelectionResult
        {
            ModOrGame = physicalPlayableObject,
            FallbackGame = coercedFallbackGame
        };
    }
}