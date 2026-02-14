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
        var filteredErrors = FilteredErrors(verificationResult.Errors).OrderByDescending(x => x.Severity).ToList();
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
        if (verificationResult.Errors.Count == 0)
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

        Console.WriteLine($"TOTAL Verification Errors: {verificationResult.Errors.Count}");

        var groupedBySeverity = verificationResult.Errors.GroupBy(x => x.Severity);
        foreach (var group in groupedBySeverity) 
            Console.WriteLine($"  Severity {group.Key}: {group.Count()}");
        Console.WriteLine();

        if (filteredErrors.Count == 0)
        {
            if (verificationResult.Errors.Count != 0)
                Console.WriteLine("Some errors are not displayed to the console. Please check the created output files.");
            return;
        }

        if (Settings.SummaryOnly)
            return;

        Console.WriteLine($"Below the list of errors with severity '{Settings.MinimumReportSeverity}' or higher:");

        foreach (var error in filteredErrors)
            Console.WriteLine($"[{error.Severity}] [{error.Id}] Message={error.Message}");
    }
}