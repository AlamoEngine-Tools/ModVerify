using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.Pipeline;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifierBase : IGameVerifierInfo
{
    public event EventHandler<VerificationErrorEventArgs>? Error;

    public event EventHandler<VerifyProgressEventArgs>? Progress; 

    private readonly IGameDatabase _gameDatabase;
    private readonly ConcurrentDictionary<VerificationError, byte> _verifyErrors = new();

    protected readonly IFileSystem FileSystem;
    protected readonly IServiceProvider Services;
    protected readonly GameVerifySettings Settings;

    public IReadOnlyCollection<VerificationError> VerifyErrors => [.. _verifyErrors.Keys];

    public virtual string FriendlyName => GetType().Name;

    public string Name => GetType().FullName;

    public IGameVerifierInfo? Parent { get; }

    protected IGameDatabase Database { get; }

    protected IGameRepository Repository => _gameDatabase.GameRepository;

    protected IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    protected GameVerifierBase(
        IGameVerifierInfo? parent,
        IGameDatabase gameDatabase,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _gameDatabase = gameDatabase ?? throw new ArgumentNullException(nameof(gameDatabase)); 
        Parent = parent;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Database = gameDatabase ?? throw new ArgumentNullException(nameof(gameDatabase));
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
        Progress?.Invoke(this, new VerifyProgressEventArgs(message, progress));
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