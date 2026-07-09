namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides the settings for the JSON verification reporter.</summary>
public record JsonReporterSettings : FileBasedReporterSettings
{
    /// <summary>Gets or sets a value that indicates whether errors are aggregated in the JSON report.</summary>
    /// <value>
    /// <see langword="true"/> if errors are aggregated; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    public bool AggregateResults { get; init; }
}