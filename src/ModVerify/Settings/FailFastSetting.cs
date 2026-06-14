using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

/// <summary>
/// Represents the settings for fail-fast behavior in the verification process.
/// </summary>
public readonly struct FailFastSetting
{
    /// <summary>
    /// A <see cref="FailFastSetting"/> instance that indicates no fail-fast behavior.
    /// </summary>
    public static readonly FailFastSetting NoFailFast = default;

    /// <summary>
    /// Gets a value indicating whether fail-fast behavior is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the verification process shall stop immediately upon
    /// encountering the first verification error that meets or exceeds <see cref="MinimumSeverity"/>;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsFailFast { get; }

    /// <summary>
    /// Gets the minimum severity level of verification errors that will trigger fail-fast behavior.
    /// </summary>
    public VerificationSeverity MinimumSeverity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailFastSetting"/> struct with
    /// the specified minimum severity level and enabled fail-fast behavior.
    /// </summary>
    /// <param name="severity">The minimum severity level of verification errors that will trigger fail-fast behavior.</param>
    public FailFastSetting(VerificationSeverity severity)
    {
        IsFailFast = true;
        MinimumSeverity = severity;
    }
}