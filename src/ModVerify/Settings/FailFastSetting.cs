using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

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