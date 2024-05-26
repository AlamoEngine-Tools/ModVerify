using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Steps;
using AnakinRaW.CommonUtilities.SimplePipeline;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Engine.Pipeline;

namespace AET.ModVerify;

public class VerifyFocPipeline(IList<string> modDirectories, string gameDirectory, string fallbackGameDirectory, IServiceProvider serviceProvider)
    : ParallelPipeline(serviceProvider, 4, false)
{
    private IList<GameVerificationStep> _verificationSteps = null!;

    protected override Task<IList<IStep>> BuildSteps()
    {
        var repository = new FocGameRepository(modDirectories, gameDirectory, fallbackGameDirectory, ServiceProvider);

        var buildIndexStep = new CreateGameDatabaseStep(repository, ServiceProvider);
        _verificationSteps = new List<GameVerificationStep>
        {
            new VerifyReferencedModelsStep(buildIndexStep, repository, ServiceProvider),
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
        await base.RunAsync(token).ConfigureAwait(false);

        var stepsWithVerificationErrors = _verificationSteps.Where(x => x.VerifyErrors.Any()).ToList();
        if (stepsWithVerificationErrors.Any())
            throw new GameVerificationException(stepsWithVerificationErrors);
    }
}