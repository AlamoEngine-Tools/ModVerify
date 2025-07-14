using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;

namespace AET.ModVerify.App.Settings;

internal sealed class ModVerifyAppSettings
{
    public bool Interactive => GameInstallationsSettings.Interactive;

    public required VerifyPipelineSettings VerifyPipelineSettings { get; init; }

    public required ModVerifyReportSettings ReportSettings { get; init; }

    public required GameInstallationsSettings GameInstallationsSettings { get; init; }

    public VerificationSeverity? AppThrowsOnMinimumSeverity { get; init; }

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}