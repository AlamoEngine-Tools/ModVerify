namespace AET.ModVerify.Reporting.Reporters;

public sealed record ConsoleReporterSettings : ReporterSettings
{
    public bool SummaryOnly { get; init; }
}