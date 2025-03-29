using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.Reporting;

public sealed class VerificationReportBroker(IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(VerificationReportBroker));

    public async Task ReportAsync(IReadOnlyCollection<VerificationError> errors)
    {
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
    }
}