using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifierBase : IGameVerifierInfo
{
    public event EventHandler<VerificationErrorEventArgs>? Error;

    public event EventHandler<ProgressEventArgs<VerifyProgressInfo>>? Progress; 

    private readonly IStarWarsGameEngine _gameEngine;
    private readonly ConcurrentDictionary<VerificationError, byte> _verifyErrors = new();

    protected readonly IFileSystem FileSystem;
    protected readonly IServiceProvider Services;
    protected readonly GameVerifySettings Settings;

    public IReadOnlyCollection<VerificationError> VerifyErrors => [.. _verifyErrors.Keys];

    public virtual string FriendlyName => GetType().Name;

    public string Name => GetType().FullName;

    public IGameVerifierInfo? Parent { get; }

    protected IStarWarsGameEngine GameEngine { get; }

    protected IGameRepository Repository => _gameEngine.GameRepository;

    protected IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    protected GameVerifierBase(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine)); 
        Parent = parent;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        GameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
        VerifierChain = CreateVerifierChain();
    }

    protected void AddError(VerificationError error)
    {
        if (_verifyErrors.TryAdd(error, 0))
        {
            Error?.Invoke(this, new VerificationErrorEventArgs(error));

            if (error.Severity >= Settings.ThrowsOnMinimumSeverity)
                throw new GameVerificationException(error);
        }
    }

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

    protected void OnProgress(string message, double progress)
    {
        Progress?.Invoke(this, new(message, progress));
    }

    private IReadOnlyList<IGameVerifierInfo> CreateVerifierChain()
    {
        var verifierChain = new List<IGameVerifierInfo> { this };

        var parent = Parent;
        while (parent != null)
        {
            verifierChain.Insert(0, parent);
            parent = parent.Parent;
        }

        return verifierChain;
    }
}