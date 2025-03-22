using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AET.ModVerifyTool.GameFinder;
using AET.ModVerifyTool.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace AET.ModVerifyTool.ModSelectors;

internal class ConsoleModSelector(IServiceProvider serviceProvider) : ModSelectorBase(serviceProvider)
{
    public override GameLocations Select(GameInstallationsSettings settings, out IPhysicalPlayableObject targetObject,
        out GameEngineType? actualEngineType)
    {
        var gameResult = GameFinderService.FindGames();
        targetObject = SelectPlayableObject(gameResult);
        actualEngineType = targetObject.Game.Type.ToEngineType();
        return GetLocations(targetObject, gameResult, settings.AdditionalFallbackPaths);
    }

    private static IPhysicalPlayableObject SelectPlayableObject(GameFinderResult finderResult)
    {
        var list = new List<IPhysicalPlayableObject>();

        var game = finderResult.Game;
        list.Add(finderResult.Game);

        Console.WriteLine("=================");
        Console.WriteLine();
        Console.WriteLine($"0: {game.Name}");

        var counter = 1;

        var workshopMods = new HashSet<IPhysicalMod>(new ModEqualityComparer(false, false));

        foreach (var mod in game.Mods)
        {
            if (mod is not IPhysicalMod physicalMod)
                continue;

            var isSteam = mod.Type == ModType.Workshops;
            if (isSteam)
            {
                workshopMods.Add(physicalMod);
                continue;
            }
            Console.WriteLine($"{counter++}:\t{mod.Name}");
            list.Add(physicalMod);
        }

        if (finderResult.FallbackGame is not null)
        {
            var fallbackGame = finderResult.FallbackGame;
            list.Add(fallbackGame);

            Console.WriteLine("_________________");
            Console.WriteLine(fallbackGame.Type == GameType.Eaw
                ? $"{counter++}: {fallbackGame.Name} [Not yet supported]"
                : $"{counter++}: {fallbackGame.Name}");

            foreach (var mod in fallbackGame.Mods)
            {

                if (mod is not IPhysicalMod physicalMod)
                    continue;

                var isSteam = mod.Type == ModType.Workshops;
                if (isSteam)
                {
                    workshopMods.Add(physicalMod);
                    continue;
                }

                Console.WriteLine($"{counter++}:\t{mod.Name}");
                list.Add(physicalMod);
            }
        }

        if (workshopMods.Count > 0)
        {
            Console.WriteLine("_________________");
            Console.WriteLine("Workshop Items:");
            foreach (var mod in workshopMods)
            {
                Console.WriteLine($"{counter++}:\t{mod.Name}");
                list.Add(mod);
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("=================");

        while (true)
        {
            Console.Write("Select a game or mod: ");
            var numberString = Console.ReadLine();

            if (!int.TryParse(numberString, out var number))
                continue;
            if (number < list.Count)
                return list[number];
        }
    }
}