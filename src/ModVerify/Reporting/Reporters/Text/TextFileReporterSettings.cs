namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides the settings for the text-file verification reporter.</summary>
public sealed record TextFileReporterSettings : FileBasedReporterSettings
{
    /// <summary>Gets or sets a value that indicates whether findings are split across multiple files instead of written to a single file.</summary>
    /// <value>
    /// <see langword="true"/> to split findings across multiple files; otherwise, <see langword="false"/> to write a single file.
    /// The default is <see langword="true"/>.
    /// </value>
    public bool SplitIntoFiles { get; init; } = true;
}