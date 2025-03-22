using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifierBase : IGameVerifier
{
    public event EventHandler<VerificationErrorEventArgs>? Error;

    private readonly ConcurrentDictionary<VerificationError, byte> _verifyErrors = new();

    protected readonly IFileSystem FileSystem;
    protected readonly IServiceProvider Services;
    private readonly IGameDatabase _gameDatabase;

    protected GameVerifierBase(IGameVerifier? parent,
        IGameDatabase gameDatabase,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
    {
        _gameDatabase = gameDatabase;
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Parent = parent;
        Settings = settings;
        Database = gameDatabase ?? throw new ArgumentNullException(nameof(gameDatabase));
        VerifierChain = CreateVerifierChain();
    }

    public IReadOnlyCollection<VerificationError> VerifyErrors => [.._verifyErrors.Keys];

    public virtual string FriendlyName => GetType().Name;

    public string Name => GetType().FullName;

    public IGameVerifier? Parent { get; }

    protected GameVerifySettings Settings { get; }

    protected IGameDatabase Database { get; }

    protected IGameRepository Repository => _gameDatabase.GameRepository;

    protected IReadOnlyList<IGameVerifier> VerifierChain { get; }

    public abstract void Verify(CancellationToken token);

    protected void AddError(VerificationError error)
    {
        if (_verifyErrors.TryAdd(error, 0))
        {
            Error?.Invoke(this, new VerificationErrorEventArgs(error));
            if (Settings.AbortSettings.FailFast && error.Severity >= Settings.AbortSettings.MinimumAbortSeverity)
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

    private IReadOnlyList<IGameVerifier> CreateVerifierChain()
    {
        var verifierChain = new List<IGameVerifier> { this };

        var parent = Parent;
        while (parent != null)
        {
            verifierChain.Insert(0, parent);
            parent = parent.Parent;
        }

        return verifierChain;
    }
}