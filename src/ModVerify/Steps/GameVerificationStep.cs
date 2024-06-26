﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Engine.Pipeline;

namespace AET.ModVerify.Steps;

public abstract class GameVerificationStep(
    CreateGameDatabaseStep createDatabaseStep,
    IGameRepository repository, 
    VerificationSettings settings,
    IServiceProvider serviceProvider)
    : PipelineStep(serviceProvider)
{
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly List<VerificationError> _verifyErrors = new();

    private StreamWriter _errorLog = null!;

    public IReadOnlyCollection<VerificationError> VerifyErrors => _verifyErrors;

    protected VerificationSettings Settings { get; } = settings;

    protected GameDatabase Database { get; private set; } = null!;

    protected IGameRepository Repository => repository;

    protected abstract string LogFileName { get; }

    public abstract string Name { get; }

    protected sealed override void RunCore(CancellationToken token)
    {
        createDatabaseStep.Wait();
        Database = createDatabaseStep.GameDatabase;
        
        Logger?.LogInformation($"Running verifier '{Name}'...");
        try
        {
            _errorLog = CreateVerificationLogFile();
            RunVerification(token);
        }
        finally
        {
            Logger?.LogInformation($"Finished verifier '{Name}'");
            _errorLog.Dispose();
            _errorLog = null!;
        }
    }

    protected abstract void RunVerification(CancellationToken token);


    protected void AddError(VerificationError error)
    {
        if (!OnError(error))
        {
            Logger?.LogTrace($"Error suppressed: '{error}'");
            return;
        }
        _verifyErrors.Add(error);
        _errorLog.WriteLine(error.Message);

        if (Settings.ThrowBehavior == VerifyThrowBehavior.FailFast)
            throw new GameVerificationException(this);
    }

    protected virtual bool OnError(VerificationError error)
    {
        return true;
    }

    protected void GuardedVerify(Action action, Predicate<Exception> handleException, string errorContext)
    {
        try
        {
            action();
        }
        catch (Exception e) when (handleException(e))
        {
            AddError(VerificationError.CreateFromException(e, errorContext));
        }
    }
    private StreamWriter CreateVerificationLogFile()
    {
        var fileName = $"VerifyResult_{LogFileName}.txt";
        var fs = FileSystem.FileStream.New(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        return new StreamWriter(fs);
    }
}