using System;

namespace AET.ModVerify.Reporting;

public record FileBasedReporterSettings : VerificationReportSettings
{
    private readonly string _outputDirectory = Environment.CurrentDirectory;

    public string OutputDirectory
    {
        get => _outputDirectory;
        init => _outputDirectory = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value;
    }
}