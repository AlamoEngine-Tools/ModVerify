using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerify.Settings;

namespace AET.ModVerifyTool.Options;

internal sealed class ModVerifyAppSettings
{
    public bool Interactive => GameInstallationsSettings.Interactive;

    public required VerifyPipelineSettings VerifyPipelineSettings { get; init; }

    public required GlobalVerifyReportSettings GlobalReportSettings { get; init; }

    public required GameInstallationsSettings GameInstallationsSettings { get; init; }

    public VerificationSeverity? AppThrowsOnMinimumSeverity { get; init; }

    public string? ReportOutput { get; init; }

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }

    public bool Offline { get; init; }
}