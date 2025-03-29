using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerify.Utilities;
using AET.ModVerify.Verifiers;
using AET.ModVerifyTool.Options;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerifyTool.Pipeline;

internal sealed class VerifyGamePipeline : AnakinRaW.CommonUtilities.SimplePipeline.Pipeline
{
    private readonly List<GameVerifier> _verificationSteps = new();
    private readonly GameEngineType _targetType;
    private readonly GameLocations _gameLocations;
    private readonly ParallelStepRunner _verifyRunner;

    private readonly VerifyPipelineSettings _pipelineSettings;
    private readonly GlobalVerifyReportSettings _reportSettings;

    protected override bool FailFast { get; }

    public IReadOnlyCollection<VerificationError> FilteredErrors { get; private set; } = [];
    
    public VerifyGamePipeline(
        GameEngineType targetType, 
        GameLocations gameLocations, 
        VerifyPipelineSettings pipelineSettings, 
        GlobalVerifyReportSettings reportSettings,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _targetType = targetType;
        _gameLocations = gameLocations ?? throw new ArgumentNullException(nameof(gameLocations));
        _pipelineSettings = pipelineSettings ?? throw new ArgumentNullException(nameof(pipelineSettings));
        _reportSettings = reportSettings ?? throw new ArgumentNullException(nameof(reportSettings));

        if (pipelineSettings.ParallelVerifiers is < 0 or > 64)
            throw new ArgumentException("_pipelineSettings has invalid parallel worker number.", nameof(pipelineSettings));
        
        _verifyRunner = new ParallelStepRunner(pipelineSettings.ParallelVerifiers, serviceProvider);

        FailFast = pipelineSettings.FailFast;
    }

    protected override Task<bool> PrepareCoreAsync()
    {
        _verificationSteps.Clear();
        return Task.FromResult(true);
    }

    protected override async Task RunCoreAsync(CancellationToken token)
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

        AddStep(new GameEngineErrorCollector(initializationErrorListener, database, _pipelineSettings.GameVerifySettings, ServiceProvider));

        foreach (var gameVerificationStep in CreateVerificationSteps(database))
            AddStep(gameVerificationStep);

        try
        {
            Logger?.LogInformation("Running game verifiers...");
            _verifyRunner.Error += OnError;
            await _verifyRunner.RunAsync(token);
        }
        finally
        {
            _verifyRunner.Error -= OnError;
            Logger?.LogDebug("Game verifiers finished.");
        }

        FilteredErrors = GetReportableErrors(_verificationSteps.SelectMany(s => s.VerifyErrors)).ToList();
    }

    protected override void OnError(object sender, StepRunnerErrorEventArgs e)
    {
        if (FailFast && e.Exception is GameVerificationException v)
        {
            if (v.Errors.All(error => _reportSettings.Baseline.Contains(error) || _reportSettings.Suppressions.Suppresses(error)))
                return;
        }
        base.OnError(sender, e);
    }

    private IEnumerable<GameVerifier> CreateVerificationSteps(IGameDatabase database)
    {
        return _pipelineSettings.VerifierFactory.GetVerifiers(database, _pipelineSettings.GameVerifySettings, ServiceProvider);
    }

    private void AddStep(GameVerifier verifier)
    {
        _verifyRunner.AddStep(new GameVerifierPipelineStep(verifier, ServiceProvider));
        _verificationSteps.Add(verifier);
    }

    private IEnumerable<VerificationError> GetReportableErrors(IEnumerable<VerificationError> errors)
    {
        Logger?.LogDebug("Applying baseline and suppressions.");
        // NB: We don't filter for severity here, as the individual reporters handle that. 
        // This allows better control over what gets reported. 
        return errors.ApplyBaseline(_reportSettings.Baseline)
            .ApplySuppressions(_reportSettings.Suppressions);
    }
}