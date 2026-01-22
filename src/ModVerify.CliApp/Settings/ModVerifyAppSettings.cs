using System;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;

namespace AET.ModVerify.App.Settings;

public class AppReportSettings
{
    public VerificationSeverity MinimumReportSeverity { get; init; }
    
    public string? SuppressionsPath { get; init; }
}

public sealed class VerifyReportSettings : AppReportSettings
{
    public string? BaselinePath { get; init; }
    public bool SearchBaselineLocally { get; init; }
}

internal abstract class AppSettingsBase(AppReportSettings reportSettings)
{
    public bool IsInteractive => VerificationTargetSettings.Interactive;

    public required VerificationTargetSettings VerificationTargetSettings
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required VerifyPipelineSettings VerifyPipelineSettings
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public AppReportSettings ReportSettings { get; } = reportSettings ?? throw new ArgumentNullException(nameof(reportSettings));
}

internal abstract class AppSettingsBase<T>(T reportSettings) : AppSettingsBase(reportSettings)
    where T : AppReportSettings
{
    public new T ReportSettings { get; } = reportSettings ?? throw new ArgumentNullException(nameof(reportSettings));
}

internal sealed class AppVerifySettings(VerifyReportSettings reportSettings) : AppSettingsBase<VerifyReportSettings>(reportSettings)
{
    public VerificationSeverity? AppFailsOnMinimumSeverity { get; init; }

    public required string ReportDirectory
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            field = value;
        }
    }
}

internal sealed class AppBaselineSettings(AppReportSettings reportSettings) : AppSettingsBase(reportSettings)
{
    public required string NewBaselinePath
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            field = value;
        }
    }

    public bool WriteLocations { get; init; } = true;
}