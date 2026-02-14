using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Utilities;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Baseline;

namespace AET.ModVerify.App;

internal sealed class VerifyAction(AppVerifySettings settings, IServiceProvider services)
    : ModVerifyApplicationAction<AppVerifySettings>(settings, services)
{
    protected override void PrintAction(VerificationTarget target)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Verifying {target.Name} for issues...");
        Console.WriteLine();
        ModVerifyConsoleUtilities.WriteSelectedTarget(target);
        Console.WriteLine();
    }

    protected override async Task<int> ProcessResult(VerificationResult result)
    {
        Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Reporting Errors...");
        var reportBroker = new VerificationReportBroker(CreateReporters(), ServiceProvider);

        result = result with
        {
            Target = result.Target with
            {
                Location = result.Target.Location.MaskUsername()
            }
        };

        await reportBroker.ReportAsync(result);

        if (Settings.AppFailsOnMinimumSeverity.HasValue &&
            result.Errors.Any(x => x.Severity >= Settings.AppFailsOnMinimumSeverity))
        {
            Logger?.LogInformation(ModVerifyConstants.ConsoleEventId,
                "The verification of {Target} completed with findings of the specified failure severity {Severity}",
                result.Target.Name, Settings.AppFailsOnMinimumSeverity);

            return ModVerifyConstants.CompletedWithFindings;
        }

        return ModVerifyConstants.Success;
    }
    
    protected override VerificationBaseline GetBaseline(VerificationTarget verificationTarget)
    {
        var baselineSelector = new BaselineSelector(Settings, ServiceProvider);
        var baseline = baselineSelector.SelectBaseline(verificationTarget, out var baselinePath);
        if (!baseline.IsEmpty)
        {
            Console.WriteLine();
            ModVerifyConsoleUtilities.WriteBaselineInfo(baseline, baselinePath);
            Logger?.LogDebug("Using baseline {Baseline} from location '{Path}'", baseline.ToString(), baselinePath);
            Console.WriteLine();
        }
        return baseline;
    }

    private IReadOnlyCollection<IVerificationReporter> CreateReporters()
    {
        var reporters = new List<IVerificationReporter>();

        reporters.Add(IVerificationReporter.CreateConsole(new ConsoleReporterSettings
        {
            MinimumReportSeverity = Settings.VerifierServiceSettings.FailFastSettings.IsFailFast
                ? VerificationSeverity.Information
                : VerificationSeverity.Error
        }, ServiceProvider));

        var outputDirectory = Settings.ReportDirectory;
        reporters.Add(IVerificationReporter.CreateJson(new JsonReporterSettings
        {
            OutputDirectory = outputDirectory,
            MinimumReportSeverity = Settings.ReportSettings.MinimumReportSeverity,
            AggregateResults = true
        }, ServiceProvider));

        reporters.Add(IVerificationReporter.CreateText(new TextFileReporterSettings
        {
            OutputDirectory = outputDirectory!,
            MinimumReportSeverity = Settings.ReportSettings.MinimumReportSeverity
        }, ServiceProvider));

        return reporters;
    }
}