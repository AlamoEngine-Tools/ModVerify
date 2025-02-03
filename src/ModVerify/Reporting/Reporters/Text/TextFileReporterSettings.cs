using AET.ModVerify.Reporting.Settings;

namespace AET.ModVerify.Reporting.Reporters.Text;

public record TextFileReporterSettings : FileBasedReporterSettings
{
    public bool SplitIntoFiles { get; init; } = true;
}