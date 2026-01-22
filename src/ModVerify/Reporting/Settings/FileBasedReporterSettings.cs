using System;

namespace AET.ModVerify.Reporting.Settings;

public record FileBasedReporterSettings : ReporterSettings
{
    public string OutputDirectory
    {
        get;
        init => field = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value;
    } = Environment.CurrentDirectory;
}