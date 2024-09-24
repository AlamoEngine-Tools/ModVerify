using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Settings;

namespace AET.ModVerify.Reporting.Reporters;

internal class ConsoleReporter(VerificationReportSettings settings, IServiceProvider serviceProvider) : ReporterBase<VerificationReportSettings>(settings, serviceProvider)
{
    public override Task ReportAsync(IReadOnlyCollection<VerificationError> errors)
    {
        var filteredErrors = FilteredErrors(errors).OrderByDescending(x => x.Severity).ToList();

        Console.WriteLine();
        Console.WriteLine("GAME VERIFICATION RESULT");
        Console.WriteLine($"Errors of severity {Settings.MinimumReportSeverity}: {filteredErrors.Count}");
        Console.WriteLine();

        if (filteredErrors.Count == 0)
            Console.WriteLine("No errors!");

        foreach (var error in filteredErrors) 
            Console.WriteLine($"[{error.Severity}] [{error.Id}] Message={error.Message}");

        Console.WriteLine();

        return Task.CompletedTask;
    }
}