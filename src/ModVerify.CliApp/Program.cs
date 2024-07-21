using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Settings;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PG.Commons.Extensibility;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.DAT.Services.Builder;
using PG.StarWarsGame.Files.MEG.Data.Archives;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace ModVerify.CliApp;

internal class Program
{
    private const string EngineParserNamespace = "PG.StarWarsGame.Engine.Xml.Parsers";
    private const string ParserNamespace = "PG.StarWarsGame.Files.XML.Parsers";

    private static async Task<int> Main(string[] args) 
    {
        var result = 0;
        
        var parseResult = Parser.Default.ParseArguments<ModVerifyOptions>(args);

        ModVerifyOptions? verifyOptions = null!;
        await parseResult.WithParsedAsync(o =>
        {
            verifyOptions = o;
            return Task.CompletedTask;
        }).ConfigureAwait(false);

        await parseResult.WithNotParsedAsync(e =>
        {
            Console.WriteLine(HelpText.AutoBuild(parseResult).ToString());
            result = 0xA0;
            return Task.CompletedTask;
        }).ConfigureAwait(false);

        if (verifyOptions is null)
        {
            if (result != 0)
                return result;
            throw new InvalidOperationException("Mod verify was executed with the wrong arguments.");
        }

        var services = CreateAppServices(verifyOptions.Verbose);
        var settings = BuildSettings(verifyOptions, services);
        var gameSetupData = CreateGameSetupData(verifyOptions, services);

        var verifier = new ModVerifyApp(settings, services);
        await verifier.RunVerification(gameSetupData).ConfigureAwait(false);

        if (verifyOptions.NewBaselineFile is not null) 
            await verifier.WriteBaseline(verifyOptions.NewBaselineFile).ConfigureAwait(false);

        return 0;
    }

    private static VerifyGameSetupData CreateGameSetupData(ModVerifyOptions options, IServiceProvider services)
    {
        var selectionResult = new ModOrGameSelector(services).SelectModOrGame(options.Path);
        
        IList<string> mods = Array.Empty<string>();
        if (selectionResult.ModOrGame is IMod mod)
        {
            var traverser = services.GetRequiredService<IModDependencyTraverser>();
            mods = traverser.Traverse(mod)
                .Select(x => x.Mod)
                .OfType<IPhysicalMod>().Select(x => x.Directory.FullName)
                .ToList();
        }

        var fallbackPaths = new List<string>();
        if (selectionResult.FallbackGame is not null)
            fallbackPaths.Add(selectionResult.FallbackGame.Directory.FullName);

        if (!string.IsNullOrEmpty(options.AdditionalFallbackPath))
            fallbackPaths.Add(options.AdditionalFallbackPath);
        
        var gameLocations = new GameLocations(
            mods, 
            selectionResult.ModOrGame.Game.Directory.FullName,
            fallbackPaths);

        return new VerifyGameSetupData
        {
            EngineType = GameEngineType.Foc,
            GameLocations = gameLocations,
            VerifyObject = selectionResult.ModOrGame,
        };
    }

    private static GameVerifySettings BuildSettings(ModVerifyOptions options, IServiceProvider services)
    {
        var settings = GameVerifySettings.Default;

        var reportSettings = settings.GlobalReportSettings;

        if (options.Baseline is not null)
        {
            using var fs = services.GetRequiredService<IFileSystem>().FileStream
                .New(options.Baseline, FileMode.Open, FileAccess.Read);
            var baseline = VerificationBaseline.FromJson(fs);

            reportSettings = reportSettings with
            {
                Baseline = baseline
            };
        }

        if (options.Suppressions is not null)
        {
            using var fs = services.GetRequiredService<IFileSystem>().FileStream
                .New(options.Suppressions, FileMode.Open, FileAccess.Read);
            var baseline = SuppressionList.FromJson(fs);

            reportSettings = reportSettings with
            {
                Suppressions = baseline
            };
        }

        return settings with
        {
            GlobalReportSettings = reportSettings
        };
    }


    private static IServiceProvider CreateAppServices(bool verbose)
    {
        var fileSystem = new FileSystem();
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<IRegistry>(new WindowsRegistry());
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);

        serviceCollection.AddLogging(builder => ConfigureLogging(builder, fileSystem, verbose));

        SteamAbstractionLayer.InitializeServices(serviceCollection);
        PetroglyphGameClients.InitializeServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);

        RuntimeHelpers.RunClassConstructor(typeof(IDatBuilder).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(IMegArchive).TypeHandle);
        AloServiceContribution.ContributeServices(serviceCollection);
        serviceCollection.CollectPgServiceContributions();
        XmlServiceContribution.ContributeServices(serviceCollection);

        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
        ModVerifyServiceContribution.ContributeServices(serviceCollection);
        serviceCollection.RegisterConsoleReporter();
        serviceCollection.RegisterJsonReporter();
        serviceCollection.RegisterTextFileReporter();

        return serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder, IFileSystem fileSystem, bool verbose)
    {
        loggingBuilder.ClearProviders();

        // ReSharper disable once RedundantAssignment
        var logLevel = LogLevel.Information;
#if DEBUG
        logLevel = LogLevel.Debug;
        loggingBuilder.AddDebug();
#else
        if (verbose)
        {
            logLevel = LogLevel.Debug;
            loggingBuilder.AddDebug();
        }
#endif
        loggingBuilder.SetMinimumLevel(logLevel);

        SetupXmlParseLogging(loggingBuilder, fileSystem);

        loggingBuilder.AddFilter<ConsoleLoggerProvider>((category, level) =>
        {
            if (level < logLevel)
                return false;
            if (string.IsNullOrEmpty(category))
                return false;
            if (category.StartsWith(EngineParserNamespace) || category.StartsWith(ParserNamespace))
                return false;
            return true;
        }).AddConsole();
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
    }

    private static bool IsXmlParserLogging(LogEvent logEvent)
    {
        return Matching.FromSource(ParserNamespace)(logEvent) || Matching.FromSource(EngineParserNamespace)(logEvent);
    }
}