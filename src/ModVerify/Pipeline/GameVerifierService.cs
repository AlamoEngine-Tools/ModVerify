using System;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Progress;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

internal sealed class GameVerifierService(IServiceProvider serviceProvider) : IGameVerifierService
{
    public async Task<VerificationResult> VerifyAsync(
        VerificationTarget verificationTarget, 
        VerifierServiceSettings settings,
        VerificationBaseline baseline,
        SuppressionList suppressions,
        IVerifyProgressReporter? progressReporter, 
        IGameEngineInitializationReporter? engineInitializationReporter,
        CancellationToken token = default)
    {
        if (verificationTarget == null)
            throw new ArgumentNullException(nameof(verificationTarget));
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        using var pipeline = new GameVerifyPipeline(
            verificationTarget, 
            settings, 
            serviceProvider,
            baseline,
            suppressions,
            progressReporter,
            engineInitializationReporter);

        VerificationCompletionStatus completionStatus;
        var start = DateTime.UtcNow;
        
        try
        {
            await pipeline.RunAsync(token).ConfigureAwait(false);
            completionStatus = VerificationCompletionStatus.Completed;
        }
        catch (OperationCanceledException)
        {
            completionStatus = settings.FailFastSettings.IsFailFast
                ? VerificationCompletionStatus.CompletedFailFast 
                : VerificationCompletionStatus.Cancelled;
        }

        var duration = DateTime.UtcNow - start;

        return new VerificationResult
        {
            Duration = duration,
            Errors = pipeline.Errors,
            Status = completionStatus,
            Target = verificationTarget,
            UsedBaseline = baseline,
            UsedSuppressions = suppressions,
            Verifiers = pipeline.Verifiers
        };
    }
}