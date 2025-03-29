using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Settings;

namespace AET.ModVerify.Reporting.Reporters;

internal class ConsoleReporter(
    VerifyReportSettings settings, 
    bool summaryOnly,
    IServiceProvider serviceProvider) : 
    ReporterBase<VerifyReportSettings>(settings, serviceProvider)
{
    public override Task ReportAsync(IReadOnlyCollection<VerificationError> errors)
    {
        var filteredErrors = FilteredErrors(errors).OrderByDescending(x => x.Severity).ToList();
        PrintErrorStats(errors, filteredErrors);
        Console.WriteLine();
        return Task.CompletedTask;
    }

    private void PrintErrorStats(IReadOnlyCollection<VerificationError> errors, List<VerificationError> filteredErrors)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("***********************");
        Console.WriteLine("      Error Report     ");
        Console.WriteLine("***********************");
        Console.WriteLine();
        if (errors.Count == 0)
        {
            if (summaryOnly)
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

        Console.WriteLine($"TOTAL Verification Errors: {errors.Count}");

        var groupedBySeverity = errors.GroupBy(x => x.Severity);
        foreach (var group in groupedBySeverity) 
            Console.WriteLine($"  Severity {group.Key}: {group.Count()}");
        Console.WriteLine();

        if (filteredErrors.Count == 0)
        {
            if (errors.Count != 0)
                Console.WriteLine("Some errors are not displayed to the console. Please check the created output files.");
            return;
        }

        if (summaryOnly)
            return;

        Console.WriteLine($"Below the list of errors with severity '{Settings.MinimumReportSeverity}' or higher:");

        foreach (var error in filteredErrors)
            Console.WriteLine($"[{error.Severity}] [{error.Id}] Message={error.Message}");
    }
}