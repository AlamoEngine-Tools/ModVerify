namespace AET.ModVerify.Reporting.Settings;

public record VerificationReportSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;
}