using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Utilities;
using AnakinRaW.ApplicationBase;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace AET.ModVerify.App.TargetSelectors;

internal class ConsoleSelector(IServiceProvider serviceProvider) : VerificationTargetSelectorBase(serviceProvider)
{
    internal override SelectionResult SelectTarget(VerificationTargetSettings settings)
    {
        var gameResult = GameFinderService.FindGames(GameFinderSettings.Default);
        var targetObject = SelectPlayableObject(gameResult); 
        var engine = targetObject.Game.Type.ToEngineType();
        var locations = GetLocations(targetObject, gameResult.FallbackGame, settings.AdditionalFallbackPaths);
        return new(locations, engine, targetObject);
    }

    private static IPhysicalPlayableObject SelectPlayableObject(GameFinderResult finderResult)
    {
        var list = new List<IPhysicalPlayableObject>();

        var game = finderResult.Game;
        list.Add(finderResult.Game);

        Console.WriteLine();
        ConsoleUtilities.WriteHorizontalLine();
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

            ConsoleUtilities.WriteHorizontalLine('_');
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
            ConsoleUtilities.WriteHorizontalLine('_');
            Console.WriteLine("Workshop Items:");
            foreach (var mod in workshopMods)
            {
                Console.WriteLine($"{counter++}:\t{mod.Name}");
                list.Add(mod);
            }
        }

        ConsoleUtilities.WriteHorizontalLine();

        try
        {
            var selected = ConsoleUtilities.UserQuestionOnSameLine("Select a game or mod: ",
                (string input, out int value) =>
                {
                    if (!int.TryParse(input, out value))
                        return false;
                    
                    return value <= list.Count && value >= 0;
                });
            return list[selected];
        }
        finally
        {
            Console.WriteLine();
        }
    }
}