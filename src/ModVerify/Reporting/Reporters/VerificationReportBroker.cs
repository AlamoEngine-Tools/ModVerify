using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.Reporting.Reporters;

public sealed class VerificationReportBroker : IVerificationReporter
{
    private readonly ILogger? _logger;
    private readonly IReadOnlyCollection<IVerificationReporter> _reporters;

    public VerificationReportBroker(
        IReadOnlyCollection<IVerificationReporter> reporters,
        IServiceProvider serviceProvider)
    {
        _reporters = reporters ?? throw new ArgumentNullException(nameof(reporters));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(VerificationReportBroker));
    }

    public async Task ReportAsync(VerificationResult result)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                await reporter.ReportAsync(result);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Exception while reporting verification error");
            }
        }
    }
}