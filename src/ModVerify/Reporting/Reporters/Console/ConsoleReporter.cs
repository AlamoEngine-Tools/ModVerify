using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters;

internal class ConsoleReporter(ConsoleReporterSettings settings, IServiceProvider serviceProvider) : 
    ReporterBase<ConsoleReporterSettings>(settings, serviceProvider)
{
    public override Task ReportAsync(VerificationResult verificationResult)
    {
        var filteredErrors = FilteredErrors(verificationResult.Errors.NewErrors).OrderByDescending(x => x.Severity).ToList();
        PrintErrorStats(verificationResult, filteredErrors);
        Console.WriteLine();
        return Task.CompletedTask;
    }

    private void PrintErrorStats(VerificationResult verificationResult, List<VerificationError> filteredErrors)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("***********************");
        Console.WriteLine("      Error Report     ");
        Console.WriteLine("***********************");
        Console.WriteLine();

        PrintResolvedStats(verificationResult);

        if (verificationResult.Errors.NewErrors.Count == 0)
        {
            if (Settings.SummaryOnly)
            {
                Console.WriteLine("No errors found.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No errors! Well done :)");
            }

            Console.ResetColor();
            return;
        }

        Console.WriteLine($"TOTAL Verification Errors: {verificationResult.Errors.NewErrors.Count}");

        var groupedBySeverity = verificationResult.Errors.NewErrors.GroupBy(x => x.Severity);
        foreach (var group in groupedBySeverity) 
            Console.WriteLine($"  Severity {group.Key}: {group.Count()}");
        Console.WriteLine();

        if (filteredErrors.Count == 0)
        {
            if (verificationResult.Errors.NewErrors.Count != 0)
                Console.WriteLine("Some errors are not displayed to the console. Please check the created output files.");
            return;
        }

        if (Settings.SummaryOnly)
            return;

        Console.WriteLine($"Below the list of errors with severity '{Settings.MinimumReportSeverity}' or higher:");

        foreach (var error in filteredErrors)
            Console.WriteLine($"[{error.Severity}] [{error.Id}] Message={error.Message}");
    }

    private void PrintResolvedStats(VerificationResult verificationResult)
    {
        var resolvedErrors = verificationResult.Errors.ResolvedErrors;
        var resolvedCount = resolvedErrors.Sum(x => x.Value.Count);
        if (resolvedCount == 0)
            return;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"Reduced issues: {resolvedCount} error(s) present in the baseline are no longer reported.");
        Console.ResetColor();

#if DEBUG
        const bool debugBuild = true;
#else
        const bool debugBuild = false;
#endif
        if (Settings.Verbose || debugBuild)
        {
            foreach (var baseline in resolvedErrors)
            {
                foreach (var error in baseline.Value)
                    Console.WriteLine($"  [Resolved] [{baseline.Key}] [{error.Id}] Message={error.Message}");
            }
        }

        Console.WriteLine();
    }
}