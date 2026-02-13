using System;
using System.Threading;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Progress;
using AnakinRaW.CommonUtilities;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using ShellProgressBar;

namespace AET.ModVerify.App.Reporting;

public sealed class VerifyConsoleProgressReporter(string toVerifyName, AppReportSettings reportSettings) 
    : DisposableObject, IVerifyProgressReporter
{
    private static readonly ProgressBarOptions ProgressBarOptions = new()
    {
        BackgroundColor = ConsoleColor.DarkGray,
        ForegroundColorError = ConsoleColor.DarkRed,
        ProgressCharacter = '─',
        WriteQueuedMessage = WriteQueuedMessage,
    };

    private readonly bool _verbose = reportSettings.Verbose;
    private ProgressBar? _progressBar;

    public void ReportError(string message, string? errorLine)
    {
        var progressBar = EnsureProgressBar();
        progressBar.WriteErrorLine(errorLine);
        progressBar.Message = message;
    }

    public void Report(string message, double progress)
    {
        Report(progress, message, VerifyProgress.ProgressType, default);
    }

    public void Report(double progress, string? progressText, ProgressType type, VerifyProgressInfo detailedProgress)
    {
        if (type != VerifyProgress.ProgressType)
            return;

        var progressBar = EnsureProgressBar();

        if (detailedProgress.IsDetailed) 
            progressBar.Message = progressText;

        if (progress >= 1.0)
            progressBar.Message = $"Verified '{toVerifyName}'";

        var cpb = progressBar.AsProgress<double>();
        cpb.Report(progress);
        
        if (_verbose) 
            progressBar.WriteLine(progressText);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _progressBar?.Dispose();
        Console.WriteLine();
    }

    private ProgressBar EnsureProgressBar()
    {
        return LazyInitializer.EnsureInitialized(ref _progressBar,
            () => new ProgressBar(100, $"Verifying '{toVerifyName}'", ProgressBarOptions))!;
    }

    private static int WriteQueuedMessage(ConsoleOutLine arg)
    {
        if (string.IsNullOrEmpty(arg.Line))
            return 0;

        var writer = Console.Out;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        try
        {
            writer.WriteLine(arg.Line);
            return 1;
        }
        finally
        {
            Console.ResetColor();
        }
    }
}