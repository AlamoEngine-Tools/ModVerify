using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.Progress;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PG.StarWarsGame.Engine;
using AET.ModVerify.Verifiers.Utilities;

namespace AET.ModVerify.Verifiers;

/// <summary>Provides the base class for game verifiers, handling error collection, progress reporting, and access to the game engine.</summary>
public abstract class GameVerifierBase : IGameVerifierInfo
{
    /// <summary>Occurs when the verifier reports a new verification error.</summary>
    public event EventHandler<VerificationErrorEventArgs>? Error;

    /// <summary>Occurs when the verifier reports progress.</summary>
    public event EventHandler<ProgressEventArgs<VerifyProgressInfo>>? Progress;

    private readonly ConcurrentDictionary<VerificationError, byte> _verifyErrors = new();

    /// <summary>The file system used by the verifier.</summary>
    protected readonly IFileSystem FileSystem;

    /// <summary>The service provider used to resolve dependencies.</summary>
    protected readonly IServiceProvider Services;

    /// <summary>The settings that control the verification run.</summary>
    protected readonly GameVerifySettings Settings;

    /// <summary>The logger used by the verifier.</summary>
    protected readonly ILogger Logger;

    /// <summary>Gets the verification errors collected by this verifier.</summary>
    public IReadOnlyCollection<VerificationError> VerifyErrors => [.. _verifyErrors.Keys];

    /// <inheritdoc />
    /// <remarks>The default implementation returns the runtime type name.</remarks>
    public virtual string FriendlyName => GetType().Name;

    /// <inheritdoc />
    public string Name => GetType().FullName!;

    /// <inheritdoc />
    public IGameVerifierInfo? Parent { get; }

    /// <summary>Gets the game engine that the verifier runs against.</summary>
    protected IStarWarsGameEngine GameEngine { get; }

    /// <summary>Gets the game repository of the <see cref="GameEngine"/>.</summary>
    protected IGameRepository Repository => GameEngine.GameRepository;

    /// <inheritdoc />
    public IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }


    /// <summary>Initializes a new instance of the <see cref="GameVerifierBase"/> class as a child of the specified parent verifier.</summary>
    /// <param name="parent">The parent verifier whose engine, settings, and services are inherited.</param>
    protected GameVerifierBase(GameVerifierBase parent)
        : this(parent, parent.GameEngine, parent.Settings, parent.Services)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifierBase"/> class as a root verifier.</summary>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    protected GameVerifierBase(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
    : this (null, gameEngine, settings, serviceProvider)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifierBase"/> class.</summary>
    /// <param name="parent">The parent verifier, or <see langword="null"/> if this is a root verifier.</param>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameEngine"/>, <paramref name="settings"/>, or <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    protected GameVerifierBase(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Parent = parent;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        GameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
        VerifierChain = this.GetVerifierChain();
    }

    /// <summary>Records a verification error, raising the <see cref="Error"/> event the first time the error is seen.</summary>
    /// <param name="error">The verification error to record.</param>
    /// <exception cref="GameVerificationException">The severity of <paramref name="error"/> is at least the configured throw threshold.</exception>
    protected void AddError(VerificationError error)
    {
        if (_verifyErrors.TryAdd(error, 0))
        {
            Error?.Invoke(this, new VerificationErrorEventArgs(error));

            if (error.Severity >= Settings.ThrowsOnMinimumSeverity)
                throw new GameVerificationException(error);
        }
    }

    /// <summary>Runs the specified action, routing exceptions that match the filter to the handler.</summary>
    /// <param name="action">The verification action to run.</param>
    /// <param name="exceptionFilter">A predicate that selects which exceptions are handled; exceptions that do not match are not caught.</param>
    /// <param name="exceptionHandler">The handler invoked for an exception that matches <paramref name="exceptionFilter"/>.</param>
    protected void GuardedVerify(Action action, Predicate<Exception> exceptionFilter, Action<Exception> exceptionHandler)
    {
        try
        {
            action();
        }
        catch (Exception e) when (exceptionFilter(e))
        {
            exceptionHandler(e);
        }
    }

    /// <summary>Raises the <see cref="Progress"/> event with the specified progress value and message.</summary>
    /// <param name="progress">The progress value, between 0.0 and 1.0.</param>
    /// <param name="message">The progress message, or <see langword="null"/> if none.</param>
    protected void OnProgress(double progress, string? message)
    {
        Progress?.Invoke(this, new(progress, message));
    }
}