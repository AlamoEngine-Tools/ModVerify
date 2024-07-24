using System;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using CommandLine;
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
            result = 0xA0;
            return Task.CompletedTask;
        }).ConfigureAwait(false);

        if (verifyOptions is null)
        {
            if (result != 0)
                return result;
            throw new InvalidOperationException("Mod verify was executed with the wrong arguments.");
        }

        var coreServiceCollection = CreateCoreServices(verifyOptions.Verbose);
        var coreServices = coreServiceCollection.BuildServiceProvider();
        var logger = coreServices.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
        try
        {
            var settings = new SettingsBuilder(coreServices)
                .BuildSettings(verifyOptions);

            var services = CreateAppServices(coreServiceCollection, settings);

            var verifier = new ModVerifyApp(settings, services);

            return await verifier.RunApplication().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger?.LogCritical(e, e.Message);
            Console.WriteLine(e.Message);
            return e.HResult;
        }
    }
    
    private static IServiceCollection CreateCoreServices(bool verboseLogging)
    {
        var fileSystem = new FileSystem();
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IRegistry>(new WindowsRegistry());
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);

        serviceCollection.AddLogging(builder => ConfigureLogging(builder, fileSystem, verboseLogging));

        return serviceCollection;
    }


    private static IServiceProvider CreateAppServices(IServiceCollection serviceCollection, ModVerifyAppSettings settings)
    {
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));

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

        SetupReporting(serviceCollection, settings);
        
        return serviceCollection.BuildServiceProvider();
    }

    private static void SetupReporting(IServiceCollection serviceCollection, ModVerifyAppSettings settings)
    {
        serviceCollection.RegisterConsoleReporter();

        serviceCollection.RegisterJsonReporter(new JsonReporterSettings
        {
            OutputDirectory = settings.Output,
            MinimumReportSeverity = settings.GameVerifySettigns.GlobalReportSettings.MinimumReportSeverity
        });

        serviceCollection.RegisterTextFileReporter(new TextFileReporterSettings
        {
            OutputDirectory = settings.Output,
            MinimumReportSeverity = settings.GameVerifySettigns.GlobalReportSettings.MinimumReportSeverity
        });
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
        
        SetupFileLogging(loggingBuilder, fileSystem);

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
    
    private static void SetupFileLogging(ILoggingBuilder loggingBuilder, IFileSystem fileSystem)
    {
        var logPath = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), "ModVerify_log.txt");


        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .Filter.ByExcluding(IsXmlParserLogging)
            .WriteTo.RollingFile(
                logPath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
            .CreateLogger();

        loggingBuilder.AddSerilog(logger);
    }

    private static bool IsXmlParserLogging(LogEvent logEvent)
    {
        return Matching.FromSource(ParserNamespace)(logEvent) || Matching.FromSource(EngineParserNamespace)(logEvent);
    }
}