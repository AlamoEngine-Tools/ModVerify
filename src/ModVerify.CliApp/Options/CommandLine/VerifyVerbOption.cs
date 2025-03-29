using AET.ModVerify.Reporting;
using CommandLine;

namespace AET.ModVerifyTool.Options.CommandLine;

[Verb("verify", true, HelpText = "Verifies the specified game and reports the findings.")]
internal class VerifyVerbOption : BaseModVerifyOptions
{
    [Option('o', "outDir", Required = false, HelpText = "Directory where result files shall be stored to.")]
    public string? OutputDirectory { get; set; }

    [Option("failFast", Required = false, Default = false,
        HelpText = "When set, the application will abort on the first failure. The option also recognized the 'MinimumFailureSeverity' setting.")]
    public bool FailFast { get; set; }

    [Option("minFailSeverity", Required = false, Default = null,
        HelpText = "When set, the application return with an error, if any finding has at least the specified severity value.")]
    public VerificationSeverity? MinimumFailureSeverity { get; set; }

    [Option("ignoreAsserts", Required = false,
        HelpText = "When this flag is present, the application will not report engine assertions.")]
    public bool IgnoreAsserts { get; set; }

    [Option("baseline", Required = false, HelpText = "Path to a JSON baseline file.")]
    public string? Baseline { get; set; }
}