using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Extensibility;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.DAT.Services.Builder;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace ModVerify.CliApp;

internal class Program
{
    private static IServiceProvider _services = null!;

    static async Task Main(string[] args)
    {
        _services = CreateAppServices();

        var gameFinderResult = new ModFinderService(_services).FindAndAddModInCurrentDirectory();
        
        var game = gameFinderResult.Game;
        Console.WriteLine($"0: {game.Name}");

        var list = new List<IPlayableObject> { game };

        var counter = 1;
        foreach (var mod in game.Mods)
        {
            var isSteam = mod.Type == ModType.Workshops;
            var line = $"{counter++}: {mod.Name}";
            if (isSteam)
                line += "*";
            Console.WriteLine(line);
            list.Add(mod);
        }

        IPlayableObject? selectedObject = null;

        do
        {
            Console.Write("Select a game or mod to verify: ");
            var numberString = Console.ReadLine();

            if (int.TryParse(numberString, out var number))
            {
                if (number < list.Count)
                    selectedObject = list[number];
            }
        } while (selectedObject is null);

        Console.WriteLine($"Verifying {selectedObject.Name}...");

        var verifyPipeline = BuildPipeline(selectedObject, gameFinderResult.FallbackGame);

        try
        {
            await verifyPipeline.RunAsync();
        }
        catch (GameVerificationException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static VerifyGamePipeline BuildPipeline(IPlayableObject playableObject, IGame fallbackGame)
    {
        IList<string> mods = Array.Empty<string>();
        if (playableObject is IMod mod)
        {
            var traverser = _services.GetRequiredService<IModDependencyTraverser>();
            mods = traverser.Traverse(mod)
                .Select(x => x.Mod)
                .OfType<IPhysicalMod>().Select(x => x.Directory.FullName)
                .ToList();
        }

        var gameLocations = new GameLocations(
            mods,
            playableObject.Game.Directory.FullName,
            fallbackGame.Directory.FullName);

        var repo = _services.GetRequiredService<IGameRepositoryFactory>().Create(GameEngineType.Foc, gameLocations);

        return new VerifyGamePipeline(repo, VerificationSettings.Default, _services);
    }

    private static IServiceProvider CreateAppServices()
    {
        var fileSystem = new FileSystem();
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging(ConfigureLogging);

        serviceCollection.AddSingleton<IRegistry>(new WindowsRegistry());
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);

        SteamAbstractionLayer.InitializeServices(serviceCollection);
        PetroglyphGameClients.InitializeServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);

        RuntimeHelpers.RunClassConstructor(typeof(IDatBuilder).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(IMegArchive).TypeHandle);
        AloServiceContribution.ContributeServices(serviceCollection);
        serviceCollection.CollectPgServiceContributions();

        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();

        // ReSharper disable once RedundantAssignment
        var logLevel = LogLevel.Information;
#if DEBUG
        logLevel = LogLevel.Debug;
        loggingBuilder.AddDebug();
#endif
        loggingBuilder.AddConsole();
    }

}