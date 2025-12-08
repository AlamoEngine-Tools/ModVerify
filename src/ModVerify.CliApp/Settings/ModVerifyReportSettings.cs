using AET.ModVerify.Reporting;

namespace AET.ModVerify.App.Settings;

internal sealed class ModVerifyReportSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; }

    public string? SuppressionsPath { get; init; }

    public string? BaselinePath { get; init; }

    public bool SearchBaselineLocally { get; init; }
}