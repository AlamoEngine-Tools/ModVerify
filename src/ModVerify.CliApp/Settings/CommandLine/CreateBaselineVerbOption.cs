using CommandLine;

namespace AET.ModVerify.App.Settings.CommandLine;

[Verb("createBaseline", HelpText = "Verifies the specified game and creates a new baseline file at the specified location.")]
internal sealed class CreateBaselineVerbOption : BaseModVerifyOptions
{
    [Option('o', "outFile", Required = true, HelpText = "The file path of the new baseline file.")]
    public required string OutputFile { get; init; }
    
    [Option("skipLocation", Required = false, HelpText = "Skips writing the target location to the baseline.")]
    public bool SkipLocation { get; init; }
}