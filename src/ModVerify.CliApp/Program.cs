using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Settings.CommandLine;
using AET.ModVerify.App.Updates;
using AET.ModVerify.App.Utilities;
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
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AET.ModVerify.App.Reporting;
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

    private AppSettingsBase? _modVerifyAppSettings;
    private ApplicationUpdateOptions _updateOptions = new();
    private bool _offlineMode;
    private bool _verboseMode;
    private bool _isLaunchedWithoutArguments;
    
    protected override async Task<int> InitializeAppAsync(IReadOnlyList<string> args, IServiceProvider bootstrapServices)
    {
        await base.InitializeAppAsync(args, bootstrapServices);
        
        ModVerifyConsoleUtilities.WriteHeader(ApplicationEnvironment.AssemblyInfo.InformationalVersion);

        ModVerifyOptionsContainer parsedOptions;
        try
        {
            parsedOptions = new ModVerifyOptionsParser(ApplicationEnvironment, BootstrapLoggerFactory).Parse(args);
        }
        catch (Exception e)
        {
            Logger?.LogCritical(e, "Failed to parse commandline arguments: {Message}", e.Message);
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e);
            return e.HResult;
        }

        if (!parsedOptions.HasOptions)
            return ModVerifyConstants.ErrorBadArguments;

        if (parsedOptions.UpdateOptions is not null)
            _updateOptions = parsedOptions.UpdateOptions;

        if (parsedOptions.ModVerifyOptions?.Verbose is true || parsedOptions.UpdateOptions?.Verbose is true)
            _verboseMode = true;

        if (parsedOptions.ModVerifyOptions is null)
            return ModVerifyConstants.Success;

        _offlineMode = parsedOptions.ModVerifyOptions.OfflineMode;
        _isLaunchedWithoutArguments = parsedOptions.ModVerifyOptions.LaunchedWithoutArguments();

        try
        {
            _modVerifyAppSettings = new SettingsBuilder(bootstrapServices)
                .BuildSettings(parsedOptions.ModVerifyOptions);
        }
        catch (AppArgumentException e)
        {
            Logger?.LogCritical(e, "Invalid arguments specified by the user: {Message}", e.Message);
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e.Message);
            return e.HResult;
        }
        catch (Exception e)
        {
            Logger?.LogCritical(e, "Failed to create settings form commandline arguments: {Message}", e.Message);
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e);
            return e.HResult;
        }

        return ModVerifyConstants.Success;
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
        
        if (_modVerifyAppSettings is null)
            return;

        SteamAbstractionLayer.InitializeServices(services);
        PetroglyphGameInfrastructure.InitializeServices(services);

        services.SupportMTD();
        services.SupportMEG();
        services.SupportALO();
        services.SupportXML();
        PetroglyphCommons.ContributeServices(services);

        PetroglyphEngineServiceContribution.ContributeServices(services);
        services.AddModVerify();
        services.RegisterVerifierCache();

        services.AddSingleton<IBaselineFactory>(sp => new BaselineFactory(sp));

        if (_offlineMode)
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
        if (result != 0 || _modVerifyAppSettings is null)
            return result;
        return await new ModVerifyApplication(_modVerifyAppSettings, appServiceProvider).RunAsync().ConfigureAwait(false);
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

        if (_verboseMode)
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
        if (_offlineMode)
        {
            Logger?.LogTrace("Running in offline mode. There will be nothing to update.");
            return ModVerifyConstants.Success;
        }

        ModVerifyUpdateMode updateMode;
        
        if (_isLaunchedWithoutArguments)
            updateMode = ModVerifyUpdateMode.InteractiveUpdate;
        else
        {
            updateMode = _modVerifyAppSettings is not null 
                ? ModVerifyUpdateMode.CheckOnly 
                : ModVerifyUpdateMode.AutoUpdate;
        }
        
        try
        {
            Logger?.LogDebug("Running update with mode '{ModVerifyUpdateMode}'", updateMode);
            var modVerifyUpdater = new ModVerifyUpdater(serviceProvider);
            await modVerifyUpdater.RunUpdateProcedure(_updateOptions, updateMode).ConfigureAwait(false);
            Logger?.LogDebug("Update procedure completed successfully.");
            return ModVerifyConstants.Success;
        }
        catch (Exception e)
        {
            Logger?.LogCritical(e, e.Message);
            var action = updateMode switch
            {
                ModVerifyUpdateMode.CheckOnly => "checking for updates",
                _ => "updating"
            };
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, $"Error while {action}: {e.Message}", e.StackTrace);
            return e.HResult;
        }

    }
}