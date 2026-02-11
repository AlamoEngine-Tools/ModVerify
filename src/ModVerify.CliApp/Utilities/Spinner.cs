using AnakinRaW.CommonUtilities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AET.ModVerify.App.Utilities;

/// <summary>
/// Options for configuring a <see cref="ConsoleSpinner"/>.
/// </summary>
public sealed class ConsoleSpinnerOptions
{
    public string? RunningMessage { get; init; }
    public string? CompletedMessage { get; init; }
    public string? FailedMessage { get; init; }
    public bool HideCursor { get; init; }
    public TextWriter Writer { get; init; } = Console.Out;
    public int Interval { get; init; } = 200;
    public string[] Animation { get; init; } = ["|", "/", "-", "\\"];

    public static ConsoleSpinnerOptions Default { get; } = new();
}



internal sealed class ConsoleSpinner : IAsyncDisposable
{
    private readonly ConsoleSpinnerOptions _options;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _observedTask;
    private readonly bool _origCursorVisibility;
    private readonly string[] _animation;
    private int _frame;
    private int _lastTextLength;

    private ConsoleSpinner(Task observedTask, ConsoleSpinnerOptions options)
    {
        _observedTask = observedTask;
        _options = options;
        _animation = options.Animation;
        _origCursorVisibility = Console.CursorVisible;

        if (_options.HideCursor)
            Console.CursorVisible = false;

        SpinnerLoop().Forget();
    }

    public static async Task<T> Run<T>(Task<T> task, ConsoleSpinnerOptions? options = null)
    {
        options ??= ConsoleSpinnerOptions.Default;
        await using var spinner = new ConsoleSpinner(task, options);
        var result = await task.ConfigureAwait(false);
        return result;
    }

    public static async Task Run(Task task, ConsoleSpinnerOptions? options = null)
    {
        options ??= ConsoleSpinnerOptions.Default;
        await using var spinner = new ConsoleSpinner(task, options);
        await task.ConfigureAwait(false);
    }

    public static Task Run(Func<Task> asyncAction, ConsoleSpinnerOptions? options = null)
    {
        if (asyncAction is null) 
            throw new ArgumentNullException(nameof(asyncAction));
        return Run(asyncAction(), options);
    }

    public static Task<T> Run<T>(Func<Task<T>> asyncAction, ConsoleSpinnerOptions? options = null)
    {
        if (asyncAction is null)
            throw new ArgumentNullException(nameof(asyncAction));
        return Run(asyncAction(), options);
    }

    public static ConsoleSpinner Endless(ConsoleSpinnerOptions? options = null)
    {
        options ??= ConsoleSpinnerOptions.Default;
        var tcs = new TaskCompletionSource<object?>();
        return new ConsoleSpinner(tcs.Task, options);
    }

    private async Task SpinnerLoop()
    {
        try
        {
            while (!_cts.IsCancellationRequested && !_observedTask.IsCompleted)
            {
                await ShowFrameAsync();
                await Task.Delay(_options.Interval, _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private async Task ShowFrameAsync()
    {
        // Clear previous content if any
        if (_lastTextLength > 0)
        {
            await ClearTextAsync(_lastTextLength);
        }

        // Write new frame
        var frameChar = _animation[_frame++ % _animation.Length];
        var text = string.IsNullOrEmpty(_options.RunningMessage)
            ? frameChar
            : $"{frameChar} {_options.RunningMessage}";

        await _options.Writer.WriteAsync(text);
        await _options.Writer.FlushAsync();
        _lastTextLength = text.Length;
    }

    public async Task CleanupAndFinishAsync()
    {
        // Clear spinner content
        if (_lastTextLength > 0)
        {
            await ClearTextAsync(_lastTextLength);
        }

        // Show final message if needed
        var finalMessage = GetFinalMessage();
        if (!string.IsNullOrEmpty(finalMessage))
        {
            await _options.Writer.WriteLineAsync(finalMessage);
        }

        await _options.Writer.FlushAsync();
        Console.CursorVisible = _origCursorVisibility;
    }

    private async Task ClearTextAsync(int length)
    {
        // Use backspaces to go back, spaces to clear, backspaces to return to start
        await _options.Writer.WriteAsync(new string('\b', length));
        await _options.Writer.WriteAsync(new string(' ', length));
        await _options.Writer.WriteAsync(new string('\b', length));
    }

    private string? GetFinalMessage()
    {
        return _observedTask.IsCompleted
            ? _observedTask.IsFaulted || _observedTask.IsCanceled ? _options.FailedMessage : _options.CompletedMessage
            : null;
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAndFinishAsync();
        
        await CastAndDispose(_cts);
        await CastAndDispose(_observedTask);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}