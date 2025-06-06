using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using AET.ModVerifyTool.Utilities;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update.Options;
using AnakinRaW.ExternalUpdater;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;

namespace AET.ModVerifyTool.Settings.CommandLine;

internal sealed class ModVerifyOptionsParser
{
    private readonly ApplicationEnvironment _applicationEnvironment;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger? _logger;
    private readonly ILoggerFactory? _loggerFactory;

    [field: AllowNull, MaybeNull]
    private Type[] AvailableVerbTypes => LazyInitializer.EnsureInitialized(ref field, GetAvailableVerbTypes)!;

    public ModVerifyOptionsParser(
        ApplicationEnvironment applicationEnvironment,
        IFileSystem fileSystem,
        ILoggerFactory? loggerFactory)
    {
        _applicationEnvironment = applicationEnvironment;
        _fileSystem = fileSystem;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger(GetType());
    }

    public ModVerifySettingsContainer Parse(IReadOnlyList<string> args)
    {
        // If the application is updatable (.NET Framework) we need to remove potential arguments from the external updater
        // in order to keep strict parsing rules enabled for better user error messages.
        if (_applicationEnvironment.IsUpdatable()) 
            args = StripExternalUpdateResults(args);

        return ParseArguments(args);
    }

    private ModVerifySettingsContainer ParseArguments(IReadOnlyList<string> args)
    { 
        // Empty arguments means that we are "interactive" mode (user simply double-clicked the executable)
        if (args.Count == 0)
        {
            return new ModVerifySettingsContainer
            {
                ModVerifyAppSettings = BuildSettings(new VerifyVerbOption()),
                UpdateOptions = null
            };
        }
        
        var parseResult = Parser.Default.ParseArguments(args, AvailableVerbTypes);

        ModVerifyAppSettings? settings = null;
        ApplicationUpdateOptions? updateOptions = null;

        parseResult.WithParsed<BaseModVerifyOptions>(o => settings = BuildSettings(o));
        parseResult.WithParsed<ApplicationUpdateOptions>(o => updateOptions = o);

        parseResult.WithNotParsed(_ =>
        {
            _logger?.LogError("Unable to parse command line");
            Console.WriteLine(HelpText.AutoBuild(parseResult).ToString());
        });

        return new ModVerifySettingsContainer
        {
            ModVerifyAppSettings = settings,
            UpdateOptions = updateOptions,
        };
    }

    public static IReadOnlyList<string> StripExternalUpdateResults(IReadOnlyList<string> args)
    {
        // Parser.Default.FormatCommandLine(ResultOption) as used in ProcessTool.cs either returns
        // two argument segments or none (if Result == UpdaterNotRun)
        if (args.Count < 2)
            return args;

        // The external updater promises to append the result to the arguments.
        // Thus, it's sufficient to check the second last segment whether it matches.
        var secondLast = args[^2];
        
        return secondLast == ExternalUpdaterResultOptions.RawOptionString 
            ? [..args.Take(args.Count - 2)]
            : args;
    }


    private ModVerifyAppSettings BuildSettings(BaseModVerifyOptions options)
    {
        return new SettingsBuilder(_fileSystem, _loggerFactory).BuildSettings(options);
    }

    private Type[] GetAvailableVerbTypes()
    {
        return _applicationEnvironment.IsUpdatable()
            ? [typeof(VerifyVerbOption), typeof(CreateBaselineVerbOption), typeof(ApplicationUpdateOptions)]
            : [typeof(VerifyVerbOption), typeof(CreateBaselineVerbOption)];
    }
}