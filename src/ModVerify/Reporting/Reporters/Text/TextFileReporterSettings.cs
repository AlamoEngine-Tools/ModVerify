namespace AET.ModVerify.Reporting.Reporters;

public sealed record TextFileReporterSettings : FileBasedReporterSettings
{
    public bool SplitIntoFiles { get; init; } = true;
}