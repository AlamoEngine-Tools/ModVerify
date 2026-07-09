namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides the base settings shared by all verification reporters.</summary>
public record ReporterSettings
{
    /// <summary>Gets or sets the minimum severity a finding must have to be reported.</summary>
    /// <value>The minimum severity of reported findings. The default is <see cref="VerificationSeverity.Information"/>.</value>
    public VerificationSeverity MinimumReportSeverity { get; init; } = VerificationSeverity.Information;

    /// <summary>Gets or sets a value that indicates whether the reporter emits verbose output.</summary>
    /// <value>
    /// <see langword="true"/> if the reporter emits verbose output; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    public bool Verbose { get; init; }
}