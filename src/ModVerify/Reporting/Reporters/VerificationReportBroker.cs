using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Distributes a verification result to multiple reporters, isolating the failure of any individual reporter.</summary>
public sealed class VerificationReportBroker : IVerificationReporter
{
    private readonly ILogger? _logger;
    private readonly IReadOnlyCollection<IVerificationReporter> _reporters;

    /// <summary>Initializes a new instance of the <see cref="VerificationReportBroker"/> class.</summary>
    /// <param name="reporters">The reporters to distribute the verification result to.</param>
    /// <param name="serviceProvider">The service provider used to resolve a logger.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reporters"/> is <see langword="null"/>.</exception>
    public VerificationReportBroker(
        IReadOnlyCollection<IVerificationReporter> reporters,
        IServiceProvider serviceProvider)
    {
        _reporters = reporters ?? throw new ArgumentNullException(nameof(reporters));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(VerificationReportBroker));
    }

    /// <inheritdoc />
    /// <remarks>An exception thrown by an individual reporter is logged and does not prevent the remaining reporters from running.</remarks>
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
                _logger?.LogError(e, "Exception while reporting verification error. Reporter: {Reporter}", reporter.GetType().FullName);
            }
        }
    }
}