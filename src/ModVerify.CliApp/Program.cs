using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Steps;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using CommandLine;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PG.Commons.Extensibility;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.DAT.Services.Builder;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace ModVerify.CliApp;

internal class Program
{
    private const string EngineParserNamespace = "PG.StarWarsGame.Engine.Xml.Parsers";
    private const string ParserNamespace = "PG.StarWarsGame.Engine.Xml.Parsers";

    private static IServiceProvider _services = null!;
    private static CliOptions _options;

    private static async Task Main(string[] args)
    {
        _options = Parser.Default.ParseArguments<CliOptions>(args).WithParsed(o => { }).Value;
        _services = CreateAppServices();

        GameFinderResult gameFinderResult;
        IPlayableObject? selectedObject = null;

        if (!string.IsNullOrEmpty(_options.Path))
        {
            var fs = _services.GetService<IFileSystem>();
            if (!fs!.Directory.Exists(_options.Path))
                throw new DirectoryNotFoundException($"No directory found at {_options.Path}");

            gameFinderResult = new ModFinderService(_services).FindAndAddModInDirectory(_options.Path);
            var selectedPath = fs.Path.GetFullPath(_options.Path).ToUpper();
            selectedObject =
                (from mod1 in gameFinderResult.Game.Mods.OfType<IPhysicalMod>()
                    let modPath = fs.Path.GetFullPath(mod1.Directory.FullName)
                    where selectedPath.Equals(modPath)
                    select mod1).FirstOrDefault();
            if (selectedObject == null) throw new Exception($"The selected directory {_options.Path} is not a mod.");
        }
        else
        {
            gameFinderResult = new ModFinderService(_services).FindAndAddModInCurrentDirectory();
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

            do
            {
                Console.Write("Select a game or mod to verify: ");
                var numberString = Console.ReadLine();

                if (!int.TryParse(numberString, out var number)) continue;
                if (number < list.Count)
                    selectedObject = list[number];
            } while (selectedObject is null);
        }

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


        return new ModVerifyPipeline(GameEngineType.Foc, gameLocations, GameVerifySettings.Default, _services);
    }

    private static IServiceProvider CreateAppServices()
    {
        var fileSystem = new FileSystem();
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<IRegistry>(new WindowsRegistry());
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);

        serviceCollection.AddLogging(builder => ConfigureLogging(builder, fileSystem));

        SteamAbstractionLayer.InitializeServices(serviceCollection);
        PetroglyphGameClients.InitializeServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);

        RuntimeHelpers.RunClassConstructor(typeof(IDatBuilder).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(IMegArchive).TypeHandle);
        AloServiceContribution.ContributeServices(serviceCollection);
        serviceCollection.CollectPgServiceContributions();

        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
        ModVerifyServiceContribution.ContributeServices(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder, IFileSystem fileSystem)
    {
        loggingBuilder.ClearProviders();

        // ReSharper disable once RedundantAssignment
        var logLevel = LogLevel.Information;
#if DEBUG
        logLevel = LogLevel.Debug;
        loggingBuilder.AddDebug();
#else
        if (_options.Verbose)
        {
            logLevel = LogLevel.Debug;
            loggingBuilder.AddDebug();
        }
#endif
        loggingBuilder.AddConsole();
        loggingBuilder.SetMinimumLevel(logLevel);

        SetupXmlParseLogging(loggingBuilder, fileSystem);
    }

    private static void SetupXmlParseLogging(ILoggingBuilder loggingBuilder, IFileSystem fileSystem)
    {
        const string xmlParseLogFileName = "XmlParseLog.txt";

        fileSystem.File.TryDeleteWithRetry(xmlParseLogFileName);

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Warning()
            .Filter.ByIncludingOnly(IsXmlParserLogging)
            .WriteTo.File(xmlParseLogFileName, outputTemplate: "[{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
            .CreateLogger();

        loggingBuilder.AddSerilog(logger);

        loggingBuilder.AddFilter<ConsoleLoggerProvider>((category, _) =>
        {
            if (string.IsNullOrEmpty(category))
                return false;
            if (category.StartsWith(EngineParserNamespace) || category.StartsWith(ParserNamespace))
                return false;

            return true;
        });
    }

    private static bool IsXmlParserLogging(LogEvent logEvent)
    {
        return Matching.FromSource(ParserNamespace)(logEvent) || Matching.FromSource(EngineParserNamespace)(logEvent);
    }


    internal class CliOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('p', "path", Required = false, HelpText = "The path to a mod directory to verify.")]
        public string Path { get; set; }
    }
}

internal class ModVerifyPipeline(
    GameEngineType targetType,
    GameLocations gameLocations,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : VerifyGamePipeline(targetType, gameLocations, settings, serviceProvider)
{
    protected override IEnumerable<GameVerificationStep> CreateVerificationSteps(IGameDatabase database)
    {
        var verifyProvider = ServiceProvider.GetRequiredService<IVerificationProvider>();
        return verifyProvider.GetAllDefaultVerifiers(database, Settings);
    }
}