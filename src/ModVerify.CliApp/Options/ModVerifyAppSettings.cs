using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;

namespace AET.ModVerifyTool.Options;

internal sealed class ModVerifyAppSettings
{
    public required VerifyPipelineSettings VerifyPipelineSettings { get; init; }

    public required GlobalVerifyReportSettings GlobalReportSettings { get; init; }

    public required GameInstallationsSettings GameInstallationsSettings { get; init; }

    public VerificationSeverity? AppThrowsOnMinimumSeverity { get; init; }

    public string? ReportOutput { get; init; }

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}