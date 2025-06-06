using CommandLine;

namespace AET.ModVerifyTool.Settings.CommandLine;

[Verb("createBaseline", HelpText = "Verifies the specified game and creates a new baseline file at the specified location.")]
internal sealed class CreateBaselineVerbOption : BaseModVerifyOptions
{
    [Option('o', "outFile", Required = true, HelpText = "The file path of the new baseline file.")]
    public required string OutputFile { get; init; }
}