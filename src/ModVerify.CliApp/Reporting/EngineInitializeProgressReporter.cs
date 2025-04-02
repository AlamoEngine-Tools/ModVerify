using System;

namespace AET.ModVerifyTool.Reporting;

internal sealed class EngineInitializeProgressReporter : IDisposable
{
    private Progress<string>? _progress;

    public EngineInitializeProgressReporter(Progress<string>? progress)
    {
        if (progress is null)
            return;
        progress.ProgressChanged += OnProgress;
    }

    private void OnProgress(object sender, string e)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(e);
        Console.ResetColor();
    }

    public void Dispose()
    {
        Console.WriteLine();
        if (_progress is not null) 
            _progress.ProgressChanged -= OnProgress;
        _progress = null;
    }
}