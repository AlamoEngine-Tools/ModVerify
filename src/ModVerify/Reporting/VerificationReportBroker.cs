using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.Reporting;

public sealed class VerificationReportBroker(GlobalVerificationReportSettings reportSettings, IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(VerificationReportBroker));

    public async Task<IReadOnlyCollection<VerificationError>> ReportAsync(IEnumerable<IGameVerifier> steps)
    {
        var suppressions = new SuppressionList(reportSettings.Suppressions);
        var errors = GetReportableErrors(steps, suppressions);
        var reporters = serviceProvider.GetServices<IVerificationReporter>();

        foreach (var reporter in reporters)
        {
            try
            {
                await reporter.ReportAsync(errors);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Exception while reporting verification error");
            }
        }

        return errors;
    }

    private IReadOnlyCollection<VerificationError> GetReportableErrors(IEnumerable<IGameVerifier> steps, SuppressionList suppressions)
    {
        var allErrors = steps.SelectMany(s => s.VerifyErrors);

        var errorsToReport = new List<VerificationError>();
        foreach (var error in allErrors)
        {
            if (reportSettings.Baseline.Contains(error))
                continue;

            if (suppressions.Suppresses(error))
                continue;

            errorsToReport.Add(error);
        }

        // NB: We don't filter for severity here, as that is something the individual reporters should handle. 
        // This allows better control over what gets reported. 
        return errorsToReport;
    }
}