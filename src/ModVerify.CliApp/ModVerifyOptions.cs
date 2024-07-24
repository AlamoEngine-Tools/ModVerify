using AET.ModVerify.Reporting;
using CommandLine;

namespace ModVerify.CliApp;

internal class ModVerifyOptions
{
    [Option('p', "path", Required = false,
        HelpText = "The path to a mod directory to verify. If not path is specified, " +
                   "the app searches for game installations and asks the user to select a game or mod.")]
    public string? Path { get; set; }

    [Option('o', "output", Required = false, HelpText = "directory where result files shall be stored to.")]
    public string? Output { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }

    [Option("baseline", Required = false, HelpText = "Path to a JSON baseline file.")]
    public string? Baseline { get; set; }

    [Option("suppressions", Required = false, HelpText = "Path to a JSON suppression file.")]
    public string? Suppressions { get; set; }

    [Option("minFailSeverity", Required = false, Default = null,
        HelpText = "When set, the application return with an error, if any finding has at least the specified severity value.")]
    public VerificationSeverity? MinimumFailureSeverity { get; set; }


    [Option("failFast", Required = false, Default = false, 
        HelpText = "When set, the application will abort on the first failure. The option also recognized the 'MinimumFailureSeverity' setting.")]
    public bool FailFast { get; set; }

    [Option("createBaseline", Required = false,
        HelpText = "When a path is specified, the tools creates a new baseline file. " +
                   "An existing file will be overwritten. " +
                   "Previous baselines are merged into the new baseline.")]
    public string? NewBaselineFile { get; set; }

    [Option("fallbackPath", Required = false, 
        HelpText = "Additional fallback path, which may contain assets that shall be included when doing the verification.")]
    public string? AdditionalFallbackPath { get; set; }
}