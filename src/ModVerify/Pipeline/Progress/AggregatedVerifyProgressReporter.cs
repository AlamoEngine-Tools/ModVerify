using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using System;
using System.Collections.Generic;

namespace AET.ModVerify.Pipeline.Progress;

internal class AggregatedVerifyProgressReporter(
    IVerifyProgressReporter progressReporter,
    IEnumerable<GameVerifierPipelineStep> steps) 
    : AggregatedProgressReporter<GameVerifierPipelineStep, VerifyProgressInfo>(progressReporter, steps)
{
    private readonly object _syncLock = new();
    private readonly IDictionary<string, double> _progressTable = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    private double _completedSize;
    private readonly HashSet<string> _completedVerifiers = new();

    protected override string GetProgressText(GameVerifierPipelineStep step, string? progressText)
    {
        return string.IsNullOrEmpty(progressText)
            ? $"Verifier '{step.GameVerifier.FriendlyName}'"
            : $"Verifier '{step.GameVerifier.FriendlyName}': {progressText}";
    }

    protected override ProgressEventArgs<VerifyProgressInfo> CalculateAggregatedProgress(
        GameVerifierPipelineStep step,
        ProgressEventArgs<VerifyProgressInfo> progress)
    {
        var key = step.GameVerifier.Name;

        var progressValue = progress.Progress;
        var stepProgress = progressValue * step.Size;

        if (progressValue >= 1.0)
            _completedVerifiers.Add(key);

        double totalProgress;
        
        lock (_syncLock)
        {
            if (!_progressTable.TryGetValue(key, out var storedProgress))
            {
                _progressTable.Add(key, stepProgress);
                _completedSize += stepProgress;
            }
            else
            {
                var deltaSize = stepProgress - storedProgress;
                _progressTable[key] = stepProgress;
                _completedSize += deltaSize;
            }
            
            totalProgress = _completedSize / TotalSize;
            totalProgress = Math.Min(totalProgress, 1.0);
        }

        if (_completedVerifiers.Count >= TotalStepCount && progressValue >= 1.0)
            totalProgress = 1.0;

        var progressInfo = new VerifyProgressInfo
        {
            TotalVerifiers = TotalStepCount,
        };
        return new ProgressEventArgs<VerifyProgressInfo>(totalProgress, progress.ProgressText, progressInfo);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _progressTable.Clear();
        _completedVerifiers.Clear();
    }
}