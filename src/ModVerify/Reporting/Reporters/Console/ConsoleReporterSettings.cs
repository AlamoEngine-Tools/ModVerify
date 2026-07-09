namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides the settings for the console verification reporter.</summary>
public sealed record ConsoleReporterSettings : ReporterSettings
{
    /// <summary>Gets or sets a value that indicates whether only a summary is written to the console instead of individual findings.</summary>
    /// <value>
    /// <see langword="true"/> to write only a summary; otherwise, <see langword="false"/> to write individual findings.
    /// The default is <see langword="false"/>.
    /// </value>
    public bool SummaryOnly { get; init; }
}