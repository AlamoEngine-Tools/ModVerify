using System;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Progress;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify;

internal sealed class GameVerifierPipelineStep(
    GameVerifier verifier,
    IServiceProvider serviceProvider) 
    : PipelineStep(serviceProvider), IProgressStep<VerifyProgressInfo>
{
    public event EventHandler<ProgressEventArgs<VerifyProgressInfo>>? Progress;

    internal GameVerifier GameVerifier { get; } = verifier ?? throw new ArgumentNullException(nameof(verifier));

    public ProgressType Type => VerifyProgress.ProgressType;

    public long Size => 1;

    protected override Task RunCoreAsync(CancellationToken token)
    {
        try
        {
            Logger?.LogDebug("Running verifier '{Name}'...", GameVerifier.FriendlyName);
            ReportProgress(new ProgressEventArgs<VerifyProgressInfo>(0.0, "Started"));

            GameVerifier.Progress += OnVerifyProgress;
            GameVerifier.Verify(token);

            Logger?.LogDebug("Finished verifier '{Name}'", GameVerifier.FriendlyName);
            ReportProgress(new ProgressEventArgs<VerifyProgressInfo>(1.0, "Finished"));
            return Task.CompletedTask;
        }
        finally
        {
            GameVerifier.Progress += OnVerifyProgress;
        }
    }

    private void OnVerifyProgress(object _, ProgressEventArgs<VerifyProgressInfo> e)
    {
        if (e.Progress > 1.0) 
            e = new ProgressEventArgs<VerifyProgressInfo>(1.0, e.ProgressText, e.ProgressInfo);
        ReportProgress(e);
    }

    private void ReportProgress(ProgressEventArgs<VerifyProgressInfo> e)
    {
        Progress?.Invoke(this, e);
    }
}