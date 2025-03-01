namespace AET.ModVerify.Reporting.Settings;

public record GlobalVerificationReportSettings : VerificationReportSettings
{
    public VerificationBaseline Baseline { get; init; } = VerificationBaseline.Empty;

    public SuppressionList Suppressions { get; init; } = SuppressionList.Empty;

    public bool ReportAsserts { get; init; } = true;
}