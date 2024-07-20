namespace AET.ModVerify.Reporting;

public record VerificationReportSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;
}