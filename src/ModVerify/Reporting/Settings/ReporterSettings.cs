namespace AET.ModVerify.Reporting.Settings;

public record ReporterSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;
}