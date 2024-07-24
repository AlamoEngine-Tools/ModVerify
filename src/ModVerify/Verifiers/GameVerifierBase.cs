using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Repositories;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifierBase(
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : PipelineStep(serviceProvider), IGameVerifier
{

    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    private readonly HashSet<VerificationError> _verifyErrors = new();
    
    public IReadOnlyCollection<VerificationError> VerifyErrors => _verifyErrors;

    protected GameVerifySettings Settings { get; } = settings;

    protected IGameDatabase Database { get; } = gameDatabase ?? throw new ArgumentNullException(nameof(gameDatabase));

    protected IGameRepository Repository => gameDatabase.GameRepository;

    public virtual string FriendlyName => GetType().Name;

    public string Name => GetType().FullName;

    protected sealed override void RunCore(CancellationToken token)
    {
        Logger?.LogInformation($"Running verifier '{FriendlyName}'...");
        try
        {
            RunVerification(token);
        }
        finally
        {
            Logger?.LogInformation($"Finished verifier '{FriendlyName}'");
        }
    }

    protected abstract void RunVerification(CancellationToken token);

    protected void AddError(VerificationError error)
    {
        _verifyErrors.Add(error);
        if (Settings.AbortSettings.FailFast && error.Severity >= Settings.AbortSettings.MinimumAbortSeverity)
            throw new GameVerificationException(error);
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

    protected string GetGameStrippedPath(string path)
    {
        if (!FileSystem.Path.IsPathFullyQualified(path))
            return path;

        if (path.Length <= Repository.Path.Length)
            return path;

        if (path.StartsWith(Repository.Path, StringComparison.OrdinalIgnoreCase))
            return path.Substring(Repository.Path.Length);

        return path;
    }
}