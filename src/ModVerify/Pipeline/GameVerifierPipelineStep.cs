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
    IStepProgressReporter progressReporter,
    IServiceProvider serviceProvider) 
    : PipelineStep(serviceProvider), IProgressStep
{
    internal GameVerifier GameVerifier { get; } = verifier ?? throw new ArgumentNullException(nameof(verifier));

    public ProgressType Type => VerifyProgress.ProgressType;

    public IStepProgressReporter ProgressReporter { get; } = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

    public long Size => 1;

    protected override void RunCore(CancellationToken token)
    {
        Logger?.LogDebug($"Running verifier '{GameVerifier.FriendlyName}'...");
        try
        {
            ProgressReporter.Report(this, 0.0);
            GameVerifier.Progress += OnVerifyProgress;
            GameVerifier.Verify(token);
        }
        finally
        {
            ProgressReporter.Report(this, 1.0);
            GameVerifier.Progress += OnVerifyProgress;
            Logger?.LogDebug($"Finished verifier '{GameVerifier.FriendlyName}'");
        }
    }

    private void OnVerifyProgress(object _, VerifyProgressEventArgs e)
    {
        ProgressReporter.Report(this, e.Progress);
    }
}

public sealed class VerifyProgressEventArgs : ProgressEventArgs<VerifyProgressInfo>
{
    public VerifyProgressEventArgs(string progressText, double progress) 
        : base(progressText, progress, VerifyProgress.ProgressType)
    {
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



public interface IVerifyProgressReporter : IProgressReporter<VerifyProgressInfo>
{

}

internal class VerifyProgressReporter : IVerifyProgressReporter
{
    public void Report(string progressText, double progress, ProgressType type, VerifyProgressInfo detailedProgress)
    {
        throw new NotImplementedException();
    }
}


internal class AggregatedVerifyProgressReporter(
    IVerifyProgressReporter progressReporter,
    IEnumerable<GameVerifierPipelineStep> steps) 
    : AggregatedProgressReporter<GameVerifierPipelineStep, VerifyProgressInfo>(progressReporter, steps)
{
    protected override ProgressType Type => VerifyProgress.ProgressType;

    protected override string GetProgressText(GameVerifierPipelineStep step)
    {
        return step.GameVerifier.FriendlyName;
    }

    protected override double CalculateAggregatedProgress(GameVerifierPipelineStep task, double progress, out VerifyProgressInfo progressInfo)
    {
        progressInfo = default;
        return 0;
    }
}

public struct VerifyProgressInfo
{
    public int TotalVerifiers { get; internal set; }
}


internal class LateInitDelegatingProgressReporter : IStepProgressReporter, IDisposable
{
    private IStepProgressReporter? _innerReporter;

    public void Report(IProgressStep step, double progress)
    {
        _innerReporter?.Report(step, progress);
    }

    public void Initialize(IStepProgressReporter progressReporter)
    {
        _innerReporter = progressReporter;
    }

    public void Dispose()
    {
        _innerReporter = null;
    }
}
