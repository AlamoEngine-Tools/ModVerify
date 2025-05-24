using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerifyTool.Options;
using AET.ModVerifyTool.Options.CommandLine;
using AET.SteamAbstraction;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework.Json;
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
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog.Expressions;
using Testably.Abstractions;
using ILogger = Serilog.ILogger;

namespace AET.ModVerifyTool;

internal class Program : SelfUpdateableAppLifecycle
{
    private static readonly string EngineParserNamespace = typeof(XmlObjectParser<>).Namespace!;
    private static readonly string ParserNamespace = typeof(PetroglyphXmlFileParser<>).Namespace!;
    private static readonly string ModVerifyRootNameSpace = typeof(Program).Namespace!;
    private static readonly CompiledExpression PrintToConsoleExpression = SerilogExpression.Compile($"EventId.Id = {ModVerifyConstants.ConsoleEventIdValue}");

    private static ModVerifyAppSettings _modVerifySettings = null!;

    private static async Task<int> Main(string[] args) 
    {
        ModVerifyConsoleUtilities.WriteHeader();
        return await new Program().StartAsync(args);
    }

    protected override async Task<int> InitializeAppAsync(IReadOnlyList<string> args)
    {
        await base.InitializeAppAsync(args);

        try
        {
            var settings = ParseSettings(args);
            if (settings is null)
                return 0xA0;

            _modVerifySettings = settings;
            return 0;
        }
        catch (Exception e)
        {
            Logger?.LogCritical(e, $"Failed to create settings form commandline arguments: {e.Message}");
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e);
            return e.HResult;
        }
    }

    protected override void CreateAppServices(IServiceCollection services, IReadOnlyList<string> args)
    {
        base.CreateAppServices(services, args);

        services.AddLogging(ConfigureLogging);

        services.AddSingleton<IHashingService>(sp => new HashingService(sp));

        
        if (IsUpdateableApplication)
        {
#if NET
            throw new NotSupportedException();
#endif
            services.MakeAppUpdateable(
                UpdatableApplicationEnvironment,
                sp => new CosturaApplicationProductService(ApplicationEnvironment, sp),
                sp => new JsonManifestLoader(sp));
        }

        SteamAbstractionLayer.InitializeServices(services);
        PetroglyphGameInfrastructure.InitializeServices(services);

        services.SupportMTD();
        services.SupportMEG();
        services.SupportALO();
        services.SupportXML();
        PetroglyphCommons.ContributeServices(services);

        PetroglyphEngineServiceContribution.ContributeServices(services);
        services.RegisterVerifierCache();


        SetupVerifyReporting(services, _modVerifySettings);

        if (_modVerifySettings.Offline)
        {
            services.AddSingleton<IModNameResolver>(sp => new OfflineModNameResolver(sp));
            services.AddSingleton<IModGameTypeResolver>(sp => new OfflineModGameTypeResolver(sp));
        }
        else
        {
            services.AddSingleton<IModNameResolver>(sp => new OnlineModNameResolver(sp));
            services.AddSingleton<IModGameTypeResolver>(sp => new OnlineModGameTypeResolver(sp));
        }
    }

    private ModVerifyAppSettings? ParseSettings(IReadOnlyList<string> args)
    {
        Type[] programVerbs =
        [
            typeof(VerifyVerbOption),
            typeof(CreateBaselineVerbOption),
        ];

        var parseResult = Parser.Default.ParseArguments(args, programVerbs);

        ModVerifyAppSettings? settings = null;

        parseResult.WithParsed<BaseModVerifyOptions>(o => settings = BuildSettings(o));

        parseResult.WithNotParsed(_ =>
        {
            Logger?.LogError("Unable to parse command line");
            Console.WriteLine(HelpText.AutoBuild(parseResult).ToString());
        });

        return settings;
    }

    protected override ApplicationEnvironment CreateAppEnvironment()
    {
        return new ModVerifyAppEnvironment(typeof(Program).Assembly, FileSystem);
    }

    protected override IFileSystem CreateFileSystem()
    {
        return new RealFileSystem();
    }

    protected override IRegistry CreateRegistry()
    {
        return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike)
            : new WindowsRegistry();
    }

    protected override async Task<int> RunAppAsync(string[] args, IServiceProvider appServiceProvider)
    {
        return await new ModVerifyApplication(_modVerifySettings, appServiceProvider).Run().ConfigureAwait(false);
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

    private ModVerifyAppSettings BuildSettings(BaseModVerifyOptions options)
    {
        return new SettingsBuilder(FileSystem, BootstrapLoggerFactory).BuildSettings(options);
    }

    private void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();

        // ReSharper disable once RedundantAssignment
        var logLevel = LogEventLevel.Information;
#if DEBUG
        logLevel = LogEventLevel.Debug;
        loggingBuilder.AddDebug();
#endif

        if (_modVerifySettings.VerboseMode)
        {
            logLevel = LogEventLevel.Verbose;
            loggingBuilder.AddDebug();
        }

        var fileLogger = SetupFileLogging(logLevel);
        loggingBuilder.AddSerilog(fileLogger);

        var cLogger = new LoggerConfiguration()
            .WriteTo.Console(
                logLevel, 
                theme: AnsiConsoleTheme.Code, 
                outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .MinimumLevel.Is(logLevel)
            .Filter.ByIncludingOnly(x =>
            {
                // Fatal errors are handled by a global exception handler
                if (x.Level == LogEventLevel.Fatal)
                    return false;

                // Verbose should print everything we get
                if (logLevel == LogEventLevel.Verbose)
                    return true;

                // Debug should print everything that has something to do with ModVerify
                if (logLevel == LogEventLevel.Debug)
                {
                    if (!x.Properties.TryGetValue("SourceContext", out var value))
                        return false;
                    var source = value.ToString().AsSpan().Trim('\"');
                    return source.StartsWith(ModVerifyRootNameSpace.AsSpan());
                }

                // In normal operation, we only print logs, which have the print-to-console EventId set.
                return ExpressionResult.IsTrue(PrintToConsoleExpression(x));
            })
            .CreateLogger();
        loggingBuilder.AddSerilog(cLogger);
    }

    private ILogger SetupFileLogging(LogEventLevel minLevel)
    {
        var logPath = FileSystem.Path.Combine(ApplicationEnvironment.ApplicationLocalPath, "ModVerify_log.txt");

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