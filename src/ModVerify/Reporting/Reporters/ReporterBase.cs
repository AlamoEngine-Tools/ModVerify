using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Settings;

namespace AET.ModVerify.Reporting.Reporters;

public abstract class ReporterBase<T>(T settings, IServiceProvider serviceProvider) : IVerificationReporter where T : VerifyReportSettings
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    protected T Settings { get; } = settings ?? throw new ArgumentNullException(nameof(settings));

    public abstract Task ReportAsync(IReadOnlyCollection<VerificationError> errors);

    protected IEnumerable<VerificationError> FilteredErrors(IReadOnlyCollection<VerificationError> errors)
    {
        return errors.Where(x => x.Severity >= Settings.MinimumReportSeverity);
    }
}