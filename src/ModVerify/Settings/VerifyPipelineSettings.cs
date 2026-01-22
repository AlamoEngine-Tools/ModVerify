using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

public sealed class VerifyPipelineSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required IGameVerifiersProvider VerifiersProvider { get; init; }

    public FailFastSetting FailFastSettings { get; init; } = FailFastSetting.NoFailFast;

    public int ParallelVerifiers { get; init; } = 4;
}

public readonly struct FailFastSetting
{
    public static readonly FailFastSetting NoFailFast = default;

    public readonly bool IsFailFast;

    public readonly VerificationSeverity MinumumSeverity;

    public FailFastSetting(VerificationSeverity severity)
    {
        IsFailFast = true;
        MinumumSeverity = severity;
    }
}