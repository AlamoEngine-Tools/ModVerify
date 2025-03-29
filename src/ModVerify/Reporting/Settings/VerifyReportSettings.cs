namespace AET.ModVerify.Reporting.Settings;

public record VerifyReportSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;
}