using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

public record GameVerifySettings
{
    public static readonly GameVerifySettings Default = new()
    {
        AbortSettings = new(),
        GlobalReportSettings = new(),
        LocalizationOption = VerifyLocalizationOption.English
    };

    public int ParallelVerifiers { get; init; } = 4;

    public VerificationAbortSettings AbortSettings { get; init; }

    public GlobalVerificationReportSettings GlobalReportSettings { get; init; }

    public VerifyLocalizationOption LocalizationOption { get; init; }
}