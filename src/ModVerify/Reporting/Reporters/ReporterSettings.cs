namespace AET.ModVerify.Reporting.Reporters;

public record ReporterSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;
}