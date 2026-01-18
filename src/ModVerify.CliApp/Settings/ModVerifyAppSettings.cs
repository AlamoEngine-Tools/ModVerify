using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;

namespace AET.ModVerify.App.Settings;

internal sealed class ModVerifyAppSettings
{
    public bool Interactive => VerificationTargetSettings.Interactive;

    public required VerifyPipelineSettings VerifyPipelineSettings { get; init; }

    public required ModVerifyReportSettings ReportSettings { get; init; }

    public required VerificationTargetSettings VerificationTargetSettings { get; init; }

    public VerificationSeverity? AppThrowsOnMinimumSeverity { get; init; }

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}


internal enum AppMode
{
    Verify,
    Baseline
}

internal abstract class ModVerifyAppSettingsBase
{
    public abstract AppMode Mode { get; }
    public bool IsInteractive => VerificationTargetSettings.Interactive;
    public required VerificationTargetSettings VerificationTargetSettings { get; init; }
}

internal sealed class VerifyAppSettings : ModVerifyAppSettingsBase
{
    public override AppMode Mode => AppMode.Verify;
    public VerificationSeverity? AppThrowsOnMinimumSeverity { get; init; }
}

internal sealed class BaselineAppSettings : ModVerifyAppSettingsBase
{
    public override AppMode Mode => AppMode.Baseline;
    public required string NewBaselinePath { get; init; }
}