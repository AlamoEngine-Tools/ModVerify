using CommandLine;

namespace ModVerify.CliApp;

internal class ModVerifyOptions
{
    [Option('p', "path", Required = false,
        HelpText = "The path to a mod directory to verify. If not path is specified, " +
                   "the app search for mods and asks the user to select a game or mod.")]
    public string? Path { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }

    [Option("baseline", Required = false, HelpText = "Path to a JSON baseline file.")]
    public string? Baseline { get; set; }

    [Option("suppressions", Required = false, HelpText = "Path to a JSON suppression file.")]
    public string? Suppressions { get; set; }

    [Option("createBaseLine", Required = false,
        HelpText = "When a path is specified, the tools creates a new baseline file. " +
                   "An existing file will be overwritten. " +
                   "Previous baselines are merged into the new baseline.")]
    public string? NewBaselineFile { get; set; }

    [Option("fallbackPath", Required = false, 
        HelpText = "Additional fallback path, which may contain assets that shall be included when doing the verification.")]
    public string? AdditionalFallbackPath { get; set; }
}