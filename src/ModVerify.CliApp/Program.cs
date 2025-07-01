using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Settings.CommandLine;
using AET.ModVerify.App.Updates;
using AET.ModVerify.App.Utilities;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using AET.SteamAbstraction;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.ApplicationBase.Update.Options;
using AnakinRaW.AppUpdaterFramework.Json;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
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
using Serilog.Expressions;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using Testably.Abstractions;
using ILogger = Serilog.ILogger;

namespace AET.ModVerify.App;

internal class MainClass
{
    // Fody/Costura application with .NET Core apparently don't work well when the class containing the Main method are derived by a type in an embedded assembly.
    private static Task<int> Main(string[] args)
    {
        return new Program().StartAsync(args);
    }
}

internal class Program : SelfUpdateableAppLifecycle
{
    private static readonly string EngineParserNamespace = typeof(XmlObjectParser<>).Namespace!;
    private static readonly string ParserNamespace = typeof(PetroglyphXmlFileParser<>).Namespace!;
    private static readonly string ModVerifyRootNameSpace = typeof(Program).Namespace!;
    private static readonly CompiledExpression PrintToConsoleExpression = SerilogExpression.Compile($"EventId.Id = {ModVerifyConstants.ConsoleEventIdValue}");

    private static ModVerifyOptionsContainer _optionsContainer = null!;

    protected override async Task<int> InitializeAppAsync(IReadOnlyList<string> args)
    {
        ModVerifyConsoleUtilities.WriteHeader(ApplicationEnvironment.AssemblyInfo.InformationalVersion);

        await base.InitializeAppAsync(args);

        try
        {
            var settings = new ModVerifyOptionsParser(ApplicationEnvironment, BootstrapLoggerFactory).Parse(args);
            if (!settings.HasOptions)
                return 0xA0;
            _optionsContainer = settings;
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

        services.AddSingleton((ApplicationEnvironment as ModVerifyAppEnvironment)!);

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

        if (_optionsContainer.ModVerifyOptions is null)
            return;

        SteamAbstractionLayer.InitializeServices(services);
        PetroglyphGameInfrastructure.InitializeServices(services);

        services.SupportMTD();
        services.SupportMEG();
        services.SupportALO();
        services.SupportXML();
        PetroglyphCommons.ContributeServices(services);

        PetroglyphEngineServiceContribution.ContributeServices(services);
        services.RegisterVerifierCache();


        SetupVerifyReporting(services);

        if (_optionsContainer.ModVerifyOptions.OfflineMode)
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
        var result = await HandleUpdate(appServiceProvider);
        if (result != 0 || _optionsContainer.ModVerifyOptions is null)
            return result;

        ModVerifyAppSettings modVerifySettings;

        try
        {
            modVerifySettings = new SettingsBuilder(appServiceProvider).BuildSettings(_optionsContainer.ModVerifyOptions);
        }
        catch (Exception e)
        {
            Logger?.LogCritical(e, $"Failed to create settings form commandline arguments: {e.Message}");
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e);
            return e.HResult;
        }

        return await new ModVerifyApplication(modVerifySettings, appServiceProvider).Run().ConfigureAwait(false);
    }

    private void SetupVerifyReporting(IServiceCollection serviceCollection)
    {
        var options = _optionsContainer.ModVerifyOptions;
        Debug.Assert(options is not null);


        var verifyVerb = options as VerifyVerbOption;

        // Console should be in minimal summary mode if we are not in verify mode.
        var printOnlySummary = verifyVerb is null;
        
        serviceCollection.RegisterConsoleReporter(new VerifyReportSettings
        {
            MinimumReportSeverity = VerificationSeverity.Error
        }, printOnlySummary);

        if (verifyVerb == null)
            return;

        var outputDirectory = Environment.CurrentDirectory;
        
        if (!string.IsNullOrEmpty(verifyVerb.OutputDirectory))
            outputDirectory = FileSystem.Path.GetFullPath(FileSystem.Path.Combine(Environment.CurrentDirectory, verifyVerb.OutputDirectory!));

        serviceCollection.RegisterJsonReporter(new JsonReporterSettings
        {
            OutputDirectory = outputDirectory!,
            MinimumReportSeverity = options.MinimumSeverity
        });

        serviceCollection.RegisterTextFileReporter(new TextFileReporterSettings
        {
            OutputDirectory = outputDirectory!,
            MinimumReportSeverity = options.MinimumSeverity
        });
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

        if (_optionsContainer.ModVerifyOptions?.Verbose == true || _optionsContainer.UpdateOptions?.Verbose == true)
        {
            logLevel = LogEventLevel.Verbose;
            loggingBuilder.AddDebug();
        }

        var fileLogger = SetupFileLogging();
        loggingBuilder.AddSerilog(fileLogger);

        var consoleLogger = SetupConsoleLogging();
        loggingBuilder.AddSerilog(consoleLogger);

        return;

        ILogger SetupConsoleLogging()
        {
            return new LoggerConfiguration()
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
        }

        ILogger SetupFileLogging()
        {
            var logPath = FileSystem.Path.Combine(ApplicationEnvironment.ApplicationLocalPath, "ModVerify_log.txt");

            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(logLevel)
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

        static bool IsXmlParserLogging(LogEvent logEvent)
        {
            return Matching.FromSource(ParserNamespace)(logEvent) || Matching.FromSource(EngineParserNamespace)(logEvent);
        }
    }

    private async Task<int> HandleUpdate(IServiceProvider serviceProvider)
    {
        var updateOptions = _optionsContainer.UpdateOptions ?? new ApplicationUpdateOptions();
        ModVerifyUpdateMode updateMode;
        
        if (_optionsContainer.ModVerifyOptions is not null)
        {
            if (_optionsContainer.ModVerifyOptions.OfflineMode)
            {
                Logger?.LogTrace("Running in offline mode. There will be nothing to update.");
                return 0;
            }

            updateMode = _optionsContainer.ModVerifyOptions.LaunchedWithoutArguments()
                ? ModVerifyUpdateMode.InteractiveUpdate
                : ModVerifyUpdateMode.CheckOnly;
        }
        else
            updateMode = ModVerifyUpdateMode.AutoUpdate;

        return await new ModVerifyUpdater(updateOptions, serviceProvider).RunUpdateProcedure(updateMode).ConfigureAwait(false);
    }
}