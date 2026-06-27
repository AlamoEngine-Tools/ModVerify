using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Progress;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

/// <summary>
/// Defines a service for verifying a configured game installation against a set of baselines and suppressions,
/// providing progress updates and handling game engine initialization reporting.
/// </summary>
public interface IGameVerifierService
{
    /// <summary>
    /// Asynchronously verifies the specified game installation against the provided baselines and suppressions,
    /// </summary>
    /// <param name="verificationTarget">The target game installation to verify.</param>
    /// <param name="settings">The verification settings.</param>
    /// <param name="baselines">The collection of baselines against which to verify.</param>
    /// <param name="suppressions">The list of suppressions to apply during verification.</param>
    /// <param name="progressReporter">The progress reporter for handling verification progress updates.</param>
    /// <param name="engineInitializationReporter">The reporter for handling game engine initialization updates.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="Task{VerificationResult}"/> representing the asynchronous verification operation.</returns>
    Task<VerificationResult> VerifyAsync(
        VerificationTarget verificationTarget,
        VerifierServiceSettings settings,
        BaselineCollection baselines,
        SuppressionList suppressions,
        IVerifyProgressReporter? progressReporter,
        IGameEngineInitializationReporter? engineInitializationReporter,
        CancellationToken token = default);
}
