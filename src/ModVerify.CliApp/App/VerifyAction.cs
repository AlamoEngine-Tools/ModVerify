using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.App;

internal sealed class VerifyAction(AppVerifySettings settings, IServiceProvider services)
    : ModVerifyApplicationAction<AppVerifySettings>(settings, services)
{
    protected override void PrintAction(VerificationTarget target)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Verifying {target.Name} for issues...");
        Console.ResetColor();
        Console.WriteLine();
    }

    protected override async Task<int> ProcessVerifyFindings(
        VerificationTarget verificationTarget, 
        IReadOnlyCollection<VerificationError> allErrors)
    {
        Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Reporting Errors...");
        var reportBroker = new VerificationReportBroker(ServiceProvider);
        await reportBroker.ReportAsync(allErrors);

        if (Settings.AppFailsOnMinimumSeverity.HasValue &&
            allErrors.Any(x => x.Severity >= Settings.AppFailsOnMinimumSeverity))
        {
            Logger?.LogInformation(ModVerifyConstants.ConsoleEventId,
                "The verification of {Target} completed with findings of the specified failure severity {Severity}",
                verificationTarget.Name, Settings.AppFailsOnMinimumSeverity);

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
            // TODO: Handle nullable path
            Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Using baseline '{Baseline}'", baselinePath);
        }
        return baseline;
    }
}