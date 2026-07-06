namespace AET.ModVerify.Reporting.Baseline;

/// <summary>Specifies the origin of a verification baseline.</summary>
public enum BaselineSource
{
    /// <summary>The baseline was loaded from a file on disk.</summary>
    File,
    /// <summary>The baseline is the default baseline embedded in the application.</summary>
    EmbeddedDefault,
}