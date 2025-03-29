using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

public record GameVerifySettings
{
    public static readonly GameVerifySettings Default = new()
    {
        LocalizationOption = VerifyLocalizationOption.English,
        IgnoreAsserts = false,
        ThrowsOnMinimumSeverity = null
    };

    public VerificationSeverity? ThrowsOnMinimumSeverity { get; init; }

    public VerifyLocalizationOption LocalizationOption { get; init; }

    public bool IgnoreAsserts { get; init; }
}