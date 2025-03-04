using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerifyTool.Options;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.MEG;
using PG.StarWarsGame.Files.MTD;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using Testably.Abstractions;
using ILogger = Serilog.ILogger;

namespace AET.ModVerifyTool;

internal class Program
{
    private const string EngineParserNamespace = "PG.StarWarsGame.Engine.Xml.Parsers";
    private const string ParserNamespace = "PG.StarWarsGame.Files.XML.Parsers";
    private const string GameInfrastructureNamespace = "PG.StarWarsGame.Infrastructure";

    private static async Task<int> Main(string[] args) 
    {
        PrintHeader();

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

        logger?.LogDebug($"Raw command line: {Environment.CommandLine}");

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
            Console.WriteLine($"Error: {e.Message}");
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

        ModVerifyServiceContribution.ContributeServices(serviceCollection);

        SetupReporting(serviceCollection, settings);

        serviceCollection.AddSingleton<IModNameResolver>(sp => new OnlineModNameResolver(sp));
        serviceCollection.AddSingleton<IModGameTypeResolver>(sp => new OnlineModGameTypeResolver(sp));
        
        return serviceCollection.BuildServiceProvider();
    }

    private static void SetupReporting(IServiceCollection serviceCollection, ModVerifyAppSettings settings)
    {
        serviceCollection.RegisterConsoleReporter(new VerificationReportSettings
        {
            MinimumReportSeverity = VerificationSeverity.Error
        });

        serviceCollection.RegisterJsonReporter(new JsonReporterSettings
        {
            OutputDirectory = settings.Output,
            MinimumReportSeverity = settings.GameVerifySettings.GlobalReportSettings.MinimumReportSeverity
        });

        serviceCollection.RegisterTextFileReporter(new TextFileReporterSettings
        {
            OutputDirectory = settings.Output,
            MinimumReportSeverity = settings.GameVerifySettings.GlobalReportSettings.MinimumReportSeverity
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
#else
        if (verbose)
        {
            logLevel = LogLevel.Verbose;
            loggingBuilder.AddDebug();
        }
#endif
        var fileLogger = SetupFileLogging(fileSystem, logLevel);
        loggingBuilder.AddSerilog(fileLogger);

        var cLogger = new LoggerConfiguration()
            .WriteTo.Async(c =>
            {
                c.Console(logLevel, theme: AnsiConsoleTheme.Code);
            })
            .Filter.ByExcluding(x =>
            {
                if (!x.Properties.TryGetValue("SourceContext", out var value))
                    return true;
                var source = value.ToString().AsSpan().Trim('\"');

                if (source.StartsWith(EngineParserNamespace.AsSpan()))
                    return true;
                if (source.StartsWith(ParserNamespace.AsSpan()))
                    return true;
                if (source.StartsWith(GameInfrastructureNamespace.AsSpan()))
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

    private static void PrintHeader()
    {
        Console.WriteLine("***********************************");
        Console.WriteLine("***********************************");
        Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Mod Verify"));
        Console.WriteLine("***********************************");
        Console.WriteLine("***********************************");
        Console.WriteLine("                       by AnakinRaW");
        Console.WriteLine();
        Console.WriteLine();
    }
}