using AET.ModVerify.Reporting;
using CommandLine;

namespace AET.ModVerify.App.Settings.CommandLine;

[Verb("verify", HelpText = "Verifies the specified game and reports the findings.")]
internal sealed class VerifyVerbOption : BaseModVerifyOptions
{
    internal static readonly VerifyVerbOption WithoutArguments = new()
    {
        IsRunningWithoutArguments = true,
    };

    [Option('o', "outDir", Required = false, HelpText = "Directory where result files shall be stored to.")]
    public string? OutputDirectory { get; init; }

    [Option("failFast", Required = false, Default = false,
        HelpText = "When set, the application will abort on the first failure. " +
                   "The option requires 'minFailSeverity' to be set.")]
    public bool FailFast { get; init; }

    [Option("minFailSeverity", Required = false, Default = null,
        HelpText = "When set, the application returns with an error, if any finding has at least the specified severity value.")]
    public VerificationSeverity? MinimumFailureSeverity { get; set; }

    [Option("ignoreAsserts", Required = false,
        HelpText = "When this flag is present, the application will not report engine assertions.")]
    public bool IgnoreAsserts { get; init; }

    public bool IsRunningWithoutArguments { get; init; }
}
