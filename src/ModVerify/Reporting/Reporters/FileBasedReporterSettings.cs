using System;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides the settings shared by file-based verification reporters.</summary>
public record FileBasedReporterSettings : ReporterSettings
{
    /// <summary>Gets or sets the directory that report files are written to.</summary>
    /// <value>The directory that report files are written to. The default is the current working directory.</value>
    /// <remarks>Setting the value to <see langword="null"/> or an empty string resets it to the current working directory.</remarks>
    public string OutputDirectory
    {
        get;
        init => field = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value;
    } = Environment.CurrentDirectory;
}