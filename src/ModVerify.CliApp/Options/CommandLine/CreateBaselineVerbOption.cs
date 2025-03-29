using CommandLine;

namespace AET.ModVerifyTool.Options.CommandLine;

[Verb("createBaseline", HelpText = "Verifies the specified game and creates a new baseline file at the specified location.")]
internal class CreateBaselineVerbOption : BaseModVerifyOptions
{
    [Option('o', "outFile", Required = true, HelpText = "The file path of the new baseline file.")]
    public string OutputFile { get; set; }
}