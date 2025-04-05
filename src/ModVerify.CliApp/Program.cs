using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerifyTool.Options;
using AET.ModVerifyTool.Options.CommandLine;
using AET.ModVerifyTool.Updates;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Xml.Parsers;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.MEG;
using PG.StarWarsGame.Files.MTD;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Testably.Abstractions;
using ILogger = Serilog.ILogger;

namespace AET.ModVerifyTool;

internal class Program
{
    private static readonly string EngineParserNamespace = typeof(XmlObjectParser<>).Namespace!;
    private static readonly string ParserNamespace = typeof(PetroglyphXmlFileParser<>).Namespace!;
    private static readonly string ModVerifyRootNameSpace = typeof(Program).Namespace!;

    private static async Task<int> Main(string[] args) 
    {
        ConsoleUtilities.WriteHeader();

        var result = 0;
        
        Type[] programVerbs =
        [
            typeof(VerifyVerbOption),
            typeof(CreateBaselineVerbOption),
        ];

        var parseResult = Parser.Default.ParseArguments(args, programVerbs);

        await parseResult.WithParsedAsync(async o =>
        {
            result = await Run((BaseModVerifyOptions)o);
        });
        await parseResult.WithNotParsedAsync(e =>
        {
            Console.WriteLine(HelpText.AutoBuild(parseResult).ToString());
            result = 0xA0;
            return Task.CompletedTask;
        });

        return result;
    }

    private static async Task<int> Run(BaseModVerifyOptions options)
    {
        var coreServiceCollection = CreateCoreServices(options.Verbose);
        var coreServices = coreServiceCollection.BuildServiceProvider();
        var logger = coreServices.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));

        logger?.LogDebug($"Raw command line: {Environment.CommandLine}");

        try
        {
            var settings = new SettingsBuilder(coreServices).BuildSettings(options);
            var services = CreateAppServices(coreServiceCollection, settings);

            if (!settings.Offline)
                await CheckForUpdate(services, logger);


            var verifier = new ModVerifyApp(settings, services);
            return await verifier.RunApplication().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ConsoleUtilities.WriteApplicationFailure();
            logger?.LogCritical(e, e.Message);
            return e.HResult;
        }
        finally
        {
#if NET
            await Log.CloseAndFlushAsync();
#else
            Log.CloseAndFlush();
#endif
        }
    }

    private static async Task CheckForUpdate(IServiceProvider services, Microsoft.Extensions.Logging.ILogger? logger)
    {
        var updateChecker = new ModVerifyUpdaterChecker(services);

        logger?.LogDebug("Checking for available update");

        try
        {
            var updateInfo = await updateChecker.CheckForUpdateAsync().ConfigureAwait(false);
            if (updateInfo.IsUpdateAvailable)
            {
                ConsoleUtilities.WriteHorizontalLine();
                
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("New Update Available!");
                Console.ResetColor();
                
                Console.WriteLine($"Version: {updateInfo.NewVersion}, Download here: {updateInfo.DownloadLink}");
                ConsoleUtilities.WriteHorizontalLine();
                Console.WriteLine();

            }
        }
        catch(Exception e)
        {
            logger?.LogWarning($"Unable to check for updates due to an internal error: {e.Message}");
            logger?.LogTrace(e, "Checking for update failed: " + e.Message);
        }
    }

    private static IServiceCollection CreateCoreServices(bool verboseLogging)
    {
        var fileSystem = new RealFileSystem();
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
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);

        serviceCollection.SupportMTD();
        serviceCollection.SupportMEG();
        serviceCollection.SupportALO();
        serviceCollection.SupportXML();
        PetroglyphCommons.ContributeServices(serviceCollection);

        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
        serviceCollection.RegisterVerifierCache();

        SetupVerifyReporting(serviceCollection, settings);

        if (settings.Offline)
        {
            serviceCollection.AddSingleton<IModNameResolver>(sp => new OfflineModNameResolver(sp));
            serviceCollection.AddSingleton<IModGameTypeResolver>(sp => new OfflineModGameTypeResolver(sp));
        }
        else
        {
            serviceCollection.AddSingleton<IModNameResolver>(sp => new OnlineModNameResolver(sp));
            serviceCollection.AddSingleton<IModGameTypeResolver>(sp => new OnlineModGameTypeResolver(sp));
        }
        
        return serviceCollection.BuildServiceProvider();
    }

    private static void SetupVerifyReporting(IServiceCollection serviceCollection, ModVerifyAppSettings settings)
    {
        var printOnlySummary = settings.CreateNewBaseline;
        serviceCollection.RegisterConsoleReporter(new VerifyReportSettings
        {
            MinimumReportSeverity = VerificationSeverity.Error
        }, printOnlySummary);

        if (string.IsNullOrEmpty(settings.ReportOutput))
            return;

        serviceCollection.RegisterJsonReporter(new JsonReporterSettings
        {
            OutputDirectory = settings.ReportOutput!,
            MinimumReportSeverity = settings.GlobalReportSettings.MinimumReportSeverity
        });

        serviceCollection.RegisterTextFileReporter(new TextFileReporterSettings
        {
            OutputDirectory = settings.ReportOutput!,
            MinimumReportSeverity = settings.GlobalReportSettings.MinimumReportSeverity
        });
    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder, IFileSystem fileSystem, bool verbose)
    {
        loggingBuilder.ClearProviders();

        // ReSharper disable once RedundantAssignment
        var logLevel = LogEventLevel.Information;
#if DEBUG
        logLevel = LogEventLevel.Debug;
        loggingBuilder.AddDebug();
#endif

        if (verbose)
        {
            logLevel = LogEventLevel.Verbose;
            loggingBuilder.AddDebug();
        }

        var fileLogger = SetupFileLogging(fileSystem, logLevel);
        loggingBuilder.AddSerilog(fileLogger);

        var cLogger = new LoggerConfiguration()

            .WriteTo.Console(
                logLevel, 
                theme: AnsiConsoleTheme.Code, 
                outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Filter.ByIncludingOnly(x =>
            {
                if (!x.Properties.TryGetValue("SourceContext", out var value))
                    return true;
                
                var source = value.ToString().AsSpan().Trim('\"');

                if (source.StartsWith(ModVerifyRootNameSpace.AsSpan()))
                    return true;

                return false;
            })
            .CreateLogger();
        loggingBuilder.AddSerilog(cLogger);
    }

    private static ILogger SetupFileLogging(IFileSystem fileSystem, LogEventLevel minLevel)
    {
        var logPath = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), "ModVerify_log.txt");

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(minLevel)
            .Filter.ByExcluding(IsXmlParserLogging)
            .WriteTo.Async(c =>
            {
                c.RollingFile(
                    logPath,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}");
            })
            .CreateLogger();
    }

    private static bool IsXmlParserLogging(LogEvent logEvent)
    {
        return Matching.FromSource(ParserNamespace)(logEvent) || Matching.FromSource(EngineParserNamespace)(logEvent);
    }
}