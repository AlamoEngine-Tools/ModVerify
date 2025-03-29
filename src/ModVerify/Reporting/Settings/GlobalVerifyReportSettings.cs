namespace AET.ModVerify.Reporting.Settings;

public record GlobalVerifyReportSettings : VerifyReportSettings
{
    public VerificationBaseline Baseline { get; init; } = VerificationBaseline.Empty;

    public SuppressionList Suppressions { get; init; } = SuppressionList.Empty;
}