using System.Collections.Generic;
using AET.ModVerify.Reporting;
using CommandLine;
using PG.StarWarsGame.Engine;

namespace AET.ModVerifyTool.Options.CommandLine;

internal abstract class BaseModVerifyOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Sets output to verbose messages.")]
    public bool Verbose { get; set; }

    [Option("minSeverity", Required = false, Default = VerificationSeverity.Information,
        HelpText = "When set, only findings with at least the specified severity value are processed.")]
    public VerificationSeverity MinimumSeverity { get; set; }

    [Option("suppressions", Required = false, HelpText = "Path to a JSON suppression file.")]
    public string? Suppressions { get; set; }

    [Option("path", SetName = "autoDetection", Required = false, Default = null, 
        HelpText = "Specifies the path to verify. The path may be a game or mod. The application will try to find all necessary submods or base games itself. " +
                   "The argument cannot be combined with any of --mods, --game or --fallbackGame")]
    public string? AutoPath { get; set; }

    [Option("mods", SetName = "manualPaths", Required = false, Default = null, Separator = ';',
        HelpText = "The path of the mod to verify. To support submods, multiple paths can be separated using the ';' (semicolon) character. " +
                   "Leave empty, if you want to verify a game. If you want to use the interactive mode, leave this, --game and --fallbackGame empty.")]
    public IList<string>? ModPaths { get; set; }

    [Option("game", SetName = "manualPaths", Required = false, Default = null,
        HelpText = "The path of the base game. For FoC mods this points to the FoC installation, for EaW mods this points to the EaW installation. " +
                   "Leave empty, if you want to auto-detect games. If you want to use the interactive mode, leave this, --mods and --fallbackGame empty. " +
                   "If this argument is set, you also need to set --mods (including sub mods) and --fallbackGame manually.")]
    public string? GamePath { get; set; }

    [Option("fallbackGame", SetName = "manualPaths", Required = false, Default = null,
        HelpText = "The path of the fallback game. Usually this points to the EaW installation. This argument only recognized if --game is set.")]
    public string? FallbackGamePath { get; set; }


    [Option("type", Required = false, Default = null, 
        HelpText = "The game type of the mod that shall be verified. Skip this value to auto-determine the type. Valid values are 'Eaw' and 'Foc'. " +
                   "This argument is required, if the first mod of '--mods' points to a directory outside of the common folder hierarchy (e.g, /MODS/MOD_NAME or /32470/WORKSHOP_ID")]
    public GameEngineType? GameType { get; set; }


    [Option("additionalFallbackPaths", Required = false, Separator = ';',
        HelpText = "Additional fallback paths, which may contain assets that shall be included when doing the verification. Do not add EaW here. " +
                   "Multiple paths can be separated using the ';' (semicolon) character.")]
    public IList<string>? AdditionalFallbackPath { get; set; }
}