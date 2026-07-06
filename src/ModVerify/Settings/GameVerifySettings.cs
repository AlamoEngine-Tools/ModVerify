using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

/// <summary>
/// Represents the settings for game verification.
/// </summary>
public sealed record GameVerifySettings
{
    /// <summary>
    /// Gets the default <see cref="GameVerifySettings"/> instance with standard settings.
    /// </summary>
    public static readonly GameVerifySettings Default = new()
    {
        LocalizationOption = VerifyLocalizationOption.English,
        IgnoreAsserts = false,
        ThrowsOnMinimumSeverity = null
    };

    /// <summary>
    /// Gets or sets the minimum severity level of verification errors that will cause the verification process to throw an exception.
    /// </summary>
    /// <value>
    /// A <see cref="VerificationSeverity"/> value that specifies the minimum severity level of verification errors that will trigger an exception.
    /// <see langword="null"/> if no exceptions should be thrown based on verification error severity.
    /// </value>
    public VerificationSeverity? ThrowsOnMinimumSeverity { get; init; }

    /// <summary>
    /// Gets or sets the localization option to use when verifying the game.
    /// </summary>
    public VerifyLocalizationOption LocalizationOption { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether game engine assertions should be ignored during verification.
    /// </summary>
    public bool IgnoreAsserts { get; init; }
}