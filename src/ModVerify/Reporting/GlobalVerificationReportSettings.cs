using System;

namespace AET.ModVerify.Reporting;

public record GlobalVerificationReportSettings : VerificationReportSettings
{
    public VerificationBaseline Baseline { get; init; } = VerificationBaseline.Empty;

    public SuppressionList Suppressions { get; init; } = SuppressionList.Empty;
}