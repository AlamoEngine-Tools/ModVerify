namespace AET.ModVerify.Settings;

/// <summary>
/// Defines the options for localization when verifying the game.
/// </summary>
public enum VerifyLocalizationOption
{
    /// <summary>
    /// Use English localization for verification, regardless of the current system's language settings.
    /// </summary>
    English,
    /// <summary>
    /// Use the localization that gets automatically selected based on the current system's localization settings for verification.
    /// </summary>
    CurrentSystem,
    /// <summary>
    /// Use all installed localizations for verification.
    /// </summary>
    AllInstalled,
    /// <summary>
    /// Use all supported localizations for verification.
    /// </summary>
    All
}