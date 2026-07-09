using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides a base class for verification reporters that share settings and access to services.</summary>
/// <typeparam name="T">The type of settings used by the reporter.</typeparam>
/// <param name="settings">The settings that control the reporter's behavior.</param>
/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
/// <exception cref="ArgumentNullException"><paramref name="settings"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
public abstract class ReporterBase<T>(T settings, IServiceProvider serviceProvider) : IVerificationReporter where T : ReporterSettings
{
    /// <summary>Gets the service provider used to resolve dependencies.</summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>Gets the settings that control the reporter's behavior.</summary>
    protected T Settings { get; } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <inheritdoc />
    public abstract Task ReportAsync(VerificationResult verificationResult);

    /// <summary>Filters the specified errors to those meeting the configured minimum report severity.</summary>
    /// <param name="errors">The errors to filter.</param>
    /// <returns>The errors whose severity is at least <see cref="ReporterSettings.MinimumReportSeverity"/>.</returns>
    protected IEnumerable<VerificationError> FilteredErrors(IReadOnlyCollection<VerificationError> errors)
    {
        return errors.Where(x => x.Severity >= Settings.MinimumReportSeverity);
    }
}