using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AET.ModVerify.Pipeline;

public sealed class GameVerifierPipelineStep(
    GameVerifier verifier,
    IServiceProvider serviceProvider) 
    : PipelineStep(serviceProvider), IProgressStep<VerifyProgressInfo>
{
    public event EventHandler<ProgressEventArgs<VerifyProgressInfo>>? Progress;

    internal GameVerifier GameVerifier { get; } = verifier ?? throw new ArgumentNullException(nameof(verifier));

    public ProgressType Type => VerifyProgress.ProgressType;

    public long Size => 1;

    protected override void RunCore(CancellationToken token)
    {
        try
        {
            Logger?.LogDebug($"Running verifier '{GameVerifier.FriendlyName}'...");
            ReportProgress(new ProgressEventArgs<VerifyProgressInfo>("Started", 0.0));
            
            GameVerifier.Progress += OnVerifyProgress;
            GameVerifier.Verify(token);

            Logger?.LogDebug($"Finished verifier '{GameVerifier.FriendlyName}'");
            ReportProgress(new ProgressEventArgs<VerifyProgressInfo>("Finished", 1.0));
        }
        finally
        {
            GameVerifier.Progress += OnVerifyProgress;
        }
    }

    private void OnVerifyProgress(object _, ProgressEventArgs<VerifyProgressInfo> e)
    {
        if (e.Progress > 1.0) 
            e = new ProgressEventArgs<VerifyProgressInfo>(e.ProgressText, 1.0, e.ProgressInfo);
        ReportProgress(e);
    }

    private void ReportProgress(ProgressEventArgs<VerifyProgressInfo> e)
    {
        Progress?.Invoke(this, e);
    }
}

public static class VerifyProgress
{
    public static readonly ProgressType ProgressType = new()
    {
        Id = "Verify",
        DisplayName = "Verify"
    };
}

public interface IVerifyProgressReporter : IProgressReporter<VerifyProgressInfo>;

internal class AggregatedVerifyProgressReporter(
    IVerifyProgressReporter progressReporter,
    IEnumerable<GameVerifierPipelineStep> steps) 
    : AggregatedProgressReporter<GameVerifierPipelineStep, VerifyProgressInfo>(progressReporter, steps)
{
    private readonly object _syncLock = new();

    private readonly HashSet<GameVerifierPipelineStep> _completedSteps = new();

    private long _totalProgressSize;

    protected override string GetProgressText(GameVerifierPipelineStep step, string progressText)
    {
        return $"Verifier '{step.GameVerifier.FriendlyName}': {progressText}";
    }

    protected override ProgressEventArgs<VerifyProgressInfo> CalculateAggregatedProgress(
        GameVerifierPipelineStep step,
        ProgressEventArgs<VerifyProgressInfo> progress)
    {
        lock (_syncLock)
        {
            var currentStepProgressSize = (long)(progress.Progress * step.Size);
            var completed = currentStepProgressSize >= step.Size;

            var progressInfo = new VerifyProgressInfo
            {
                TotalVerifiers = TotalStepCount,
            };

            if (!completed || _completedSteps.Add(step)) 
                _totalProgressSize += (long)(progress.Progress * step.Size);
            
            return new ProgressEventArgs<VerifyProgressInfo>(progress.ProgressText, GetTotalProgress(), progressInfo);
        }
    }

    private double GetTotalProgress()
    { 
        return Math.Min((double)_totalProgressSize / TotalSize, 0.99);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _completedSteps.Clear();
    }
}

public struct VerifyProgressInfo
{
    public int TotalVerifiers { get; internal set; }
}