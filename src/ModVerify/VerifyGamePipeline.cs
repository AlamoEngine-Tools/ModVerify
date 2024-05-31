using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Steps;
using AnakinRaW.CommonUtilities.SimplePipeline;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Engine.Pipeline;

namespace AET.ModVerify;

public class VerifyGamePipeline : ParallelPipeline
{
    private IList<GameVerificationStep> _verificationSteps = null!;
    private readonly IGameRepository _repository;
    private readonly VerificationSettings _settings;

    public VerifyGamePipeline(IGameRepository gameRepository, VerificationSettings settings, IServiceProvider serviceProvider)
        : base(serviceProvider, 4, false)
    {
        _repository = gameRepository;
        _settings = settings;
    }

    protected override Task<IList<IStep>> BuildSteps()
    {
        var buildIndexStep = new CreateGameDatabaseStep(_repository, ServiceProvider);

        _verificationSteps = new List<GameVerificationStep>
        {
            new VerifyReferencedModelsStep(buildIndexStep, _repository, _settings, ServiceProvider),
        };

        var allSteps = new List<IStep>
        {
            buildIndexStep
        };
        allSteps.AddRange(_verificationSteps);

        return Task.FromResult<IList<IStep>>(allSteps);
    }

    public override async Task RunAsync(CancellationToken token = default)
    {
        Logger?.LogInformation("Verifying game...");
        try
        {
            await base.RunAsync(token).ConfigureAwait(false);

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
}