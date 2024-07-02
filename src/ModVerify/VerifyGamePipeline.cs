using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Steps;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

public abstract class VerifyGamePipeline : Pipeline
{
    private readonly List<GameVerificationStep> _verificationSteps = new();
    private readonly GameEngineType _targetType;
    private readonly GameLocations _gameLocations;
    private readonly ParallelRunner _verifyRunner;

    protected GameVerifySettings Settings { get; }

    protected VerifyGamePipeline(GameEngineType targetType, GameLocations gameLocations, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
        _targetType = targetType;
        _gameLocations = gameLocations ?? throw new ArgumentNullException(nameof(gameLocations));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (settings.ParallelWorkers is < 0 or > 64)
            throw new ArgumentException("Settings has invalid parallel worker number.", nameof(settings));
        
        _verifyRunner = new ParallelRunner(settings.ParallelWorkers, serviceProvider);
    }


    protected sealed override Task<bool> PrepareCoreAsync()
    {
        _verificationSteps.Clear();
        return Task.FromResult(true);
    }

    protected sealed override async Task RunCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Verifying game...");
        try
        {
            var databaseService = ServiceProvider.GetRequiredService<IGameDatabaseService>();

            var database = await databaseService.CreateDatabaseAsync(_targetType, _gameLocations, token);

            foreach (var gameVerificationStep in CreateVerificationSteps(database))
            {
                _verifyRunner.AddStep(gameVerificationStep);
                _verificationSteps.Add(gameVerificationStep);
            }

            try
            {
                Logger?.LogInformation("Verifying...");
                _verifyRunner.Error += OnError;
                await _verifyRunner.RunAsync(token);
            }
            finally
            {
                _verifyRunner.Error -= OnError;
                Logger?.LogInformation("Finished Verifying");
            }

            var stepsWithVerificationErrors = _verificationSteps.Where(x => x.VerifyErrors.Any()).ToList();

            var failedSteps = new List<GameVerificationStep>();
            foreach (var verificationStep in _verificationSteps)
            {
                if (verificationStep.VerifyErrors.Any())
                {
                    failedSteps.Add(verificationStep);
                    Logger?.LogWarning($"Verifier '{verificationStep.Name}' reported errors!");
                }
            }

            if (Settings.ThrowBehavior == VerifyThrowBehavior.FinalThrow && failedSteps.Count > 0)
                throw new GameVerificationException(stepsWithVerificationErrors);
        }
        finally
        {
            Logger?.LogInformation("Finished game verification");
        }
    }

    protected abstract IEnumerable<GameVerificationStep> CreateVerificationSteps(IGameDatabase database);
}