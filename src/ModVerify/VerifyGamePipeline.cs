﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

public abstract class VerifyGamePipeline : Pipeline
{
    private readonly List<GameVerifierBase> _verificationSteps = new();
    private readonly GameEngineType _targetType;
    private readonly GameLocations _gameLocations;
    private readonly ParallelStepRunner _verifyRunner;
    
    protected GameVerifySettings Settings { get; }

    protected override bool FailFast { get; }

    public IReadOnlyCollection<VerificationError> Errors { get; private set; } = Array.Empty<VerificationError>();
    
    protected VerifyGamePipeline(GameEngineType targetType, GameLocations gameLocations, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
        _targetType = targetType;
        _gameLocations = gameLocations ?? throw new ArgumentNullException(nameof(gameLocations));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (settings.ParallelVerifiers is < 0 or > 64)
            throw new ArgumentException("Settings has invalid parallel worker number.", nameof(settings));
        
        _verifyRunner = new ParallelStepRunner(settings.ParallelVerifiers, serviceProvider);
        FailFast = settings.AbortSettings.FailFast;
    }

    protected sealed override Task<bool> PrepareCoreAsync()
    {
        _verificationSteps.Clear();
        return Task.FromResult(true);
    }

    protected sealed override async Task RunCoreAsync(CancellationToken token)
    {
        var databaseService = ServiceProvider.GetRequiredService<IGameDatabaseService>();

        var initializationErrorListener = new ConcurrentGameGameErrorReporter();
        var initOptions = new GameInitializationOptions
        {
            Locations = _gameLocations,
            TargetEngineType = _targetType,
            GameErrorReporter = initializationErrorListener

        };
        var database = await databaseService.InitializeGameAsync(initOptions, token);

        AddStep(new GameEngineErrorCollector(initializationErrorListener, database, Settings, ServiceProvider));

        foreach (var gameVerificationStep in CreateVerificationSteps(database))
            AddStep(gameVerificationStep);

        try
        {
            Logger?.LogInformation("Verifying...");
            _verifyRunner.Error += OnError;
            await _verifyRunner.RunAsync(token);
        }
        finally
        {
            _verifyRunner.Error -= OnError;
            Logger?.LogInformation("Finished verification");
        }


        Logger?.LogInformation("Reporting Errors");

        var reportBroker = new VerificationReportBroker(Settings.GlobalReportSettings, ServiceProvider);
        var errors = await reportBroker.ReportAsync(_verificationSteps);

        Errors = errors;

        if (Settings.AbortSettings.ThrowsGameVerificationException &&
            errors.Any(x => x.Severity >= Settings.AbortSettings.MinimumAbortSeverity))
            throw new GameVerificationException(errors);

    }

    protected abstract IEnumerable<GameVerifierBase> CreateVerificationSteps(IGameDatabase database);

    private void AddStep(GameVerifierBase verifier)
    {
        _verifyRunner.AddStep(verifier);
        _verificationSteps.Add(verifier);
    }
}