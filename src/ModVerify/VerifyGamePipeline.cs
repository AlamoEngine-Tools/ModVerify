using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Steps;
using AnakinRaW.CommonUtilities.SimplePipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Engine.Pipeline;

namespace AET.ModVerify;

public abstract class VerifyGamePipeline : Pipeline
{
    private IList<GameVerificationStep> _verificationSteps = new List<GameVerificationStep>();
    private readonly GameLocations _gameLocations;
    private readonly VerificationSettings _settings;

    protected VerifyGamePipeline(GameLocations gameLocations, VerificationSettings settings, IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
        _gameLocations = gameLocations ?? throw new ArgumentNullException(nameof(gameLocations));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }


    protected override Task<bool> PrepareCoreAsync()
    {
        throw new NotImplementedException();
    }

    protected override async Task RunCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Verifying game...");
        try
        {
            var databaseService = ServiceProvider.GetRequiredService<IGameDatabaseService>();

            await databaseService.CreateDatabaseAsync()



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

            if (_settings.ThrowBehavior == VerifyThrowBehavior.FinalThrow && failedSteps.Count > 0)
                throw new GameVerificationException(stepsWithVerificationErrors);
        }
        finally
        {
            Logger?.LogInformation("Finished game verification");
        }
    }


    //protected sealed override async Task<IList<IStep>> BuildSteps()
    //{
    //    var buildIndexStep = new CreateGameDatabaseStep(_repository, ServiceProvider);

    //    _verificationSteps = new List<GameVerificationStep>
    //    {
    //        new VerifyReferencedModelsStep(buildIndexStep, _repository, _settings, ServiceProvider),
    //    };

    //    var allSteps = new List<IStep>
    //    {
    //        buildIndexStep
    //    };
    //    allSteps.AddRange(CreateVeificationSteps());

    //    return allSteps;
    //}


    protected abstract IEnumerable<GameVerificationStep> CreateVerificationSteps();
}