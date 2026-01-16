using AET.ModVerify.Pipeline.Progress;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Pipeline;

public sealed class GameVerifyPipeline : StepRunnerPipelineBase<AsyncStepRunner>
{
    private readonly List<GameVerifier> _verifiers = new();
    private readonly List<GameVerifierPipelineStep> _verificationSteps = new();
    private readonly ConcurrentGameEngineErrorReporter _engineErrorReporter = new();

    private readonly VerificationTarget _verificationTarget;
    private readonly VerifyPipelineSettings _pipelineSettings;
    private readonly GlobalVerifyReportSettings _reportSettings;
    private readonly IVerifyProgressReporter _progressReporter;
    private readonly IGameEngineInitializationReporter? _engineInitializationReporter;
    private readonly IPetroglyphStarWarsGameEngineService _gameEngineService;
    private readonly ILogger? _logger;
    private AggregatedVerifyProgressReporter? _aggregatedVerifyProgressReporter;

    public IReadOnlyCollection<VerificationError> FilteredErrors { get; private set; } = [];

    public GameVerifyPipeline(
        VerificationTarget verificationTarget,
        VerifyPipelineSettings pipelineSettings,
        GlobalVerifyReportSettings reportSettings,
        IVerifyProgressReporter progressReporter,
        IGameEngineInitializationReporter? engineInitializationReporter,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _verificationTarget = verificationTarget ?? throw new ArgumentNullException(nameof(verificationTarget));
        _pipelineSettings = pipelineSettings ?? throw new ArgumentNullException(nameof(pipelineSettings));
        _reportSettings = reportSettings ?? throw new ArgumentNullException(nameof(reportSettings));
        _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        _engineInitializationReporter = engineInitializationReporter;
        _gameEngineService = serviceProvider.GetRequiredService<IPetroglyphStarWarsGameEngineService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        FailFast = pipelineSettings.FailFast;
    }

    protected override AsyncStepRunner CreateRunner()
    {
        var requestedRunnerCount = _pipelineSettings.ParallelVerifiers;
        return requestedRunnerCount switch
        {
            < 0 or > 64 => throw new InvalidOperationException(
                $"Invalid parallel worker count ({requestedRunnerCount}) specified in verifier settings."),
            1 => new SequentialStepRunner(ServiceProvider),
            _ => new AsyncStepRunner(requestedRunnerCount, ServiceProvider)
        };
    }

    protected override async Task PrepareCoreAsync(CancellationToken token)
    {
        _verifiers.Clear();

        IStarWarsGameEngine gameEngine;

        try
        {
            gameEngine = await _gameEngineService.InitializeAsync(
                _verificationTarget.Engine,
                _verificationTarget.Location,
                _engineErrorReporter,
                _engineInitializationReporter,
                false,
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Creating game engine failed: {Message}", e.Message);
            throw;
        }

        AddStep(new GameEngineErrorCollector(_engineErrorReporter, gameEngine, _pipelineSettings.GameVerifySettings, ServiceProvider));

        foreach (var gameVerificationStep in CreateVerificationSteps(gameEngine))
            AddStep(gameVerificationStep);
    }

    protected override void OnExecuteStarted()
    {
        Logger?.LogInformation("Running game verifiers...");
        _aggregatedVerifyProgressReporter = new AggregatedVerifyProgressReporter(_progressReporter, _verificationSteps);
        _progressReporter.Report(0.0, $"Verifying {_verificationTarget.Name}...", VerifyProgress.ProgressType, default);
    }
    
    protected override void OnExecuteCompleted()
    {
        Logger?.LogInformation("Game verifiers finished.");
        FilteredErrors = GetReportableErrors(_verifiers.SelectMany(s => s.VerifyErrors)).ToList();
        _progressReporter.Report(1.0, $"Finished Verifying {_verificationTarget.Name}", VerifyProgress.ProgressType, default);
    }

    protected override void OnRunnerExecutionError(object sender, StepRunnerErrorEventArgs e)
    {
        if (FailFast && e.Exception is GameVerificationException v)
        {
            // TODO: Apply globalMinSeverity
            if (v.Errors.All(error => _reportSettings.Baseline.Contains(error) || _reportSettings.Suppressions.Suppresses(error)))
                return;
        }
        base.OnRunnerExecutionError(sender, e);
    }

    private void AddStep(GameVerifier verifier)
    {
        var verificationStep = new GameVerifierPipelineStep(verifier, ServiceProvider);
        StepRunner.AddStep(verificationStep);
        _verificationSteps.Add(verificationStep);
        _verifiers.Add(verifier);
    }

    private IEnumerable<VerificationError> GetReportableErrors(IEnumerable<VerificationError> errors)
    {
        Logger?.LogDebug("Applying baseline and suppressions.");
        // NB: We don't filter for severity here, as the individual reporters handle that. 
        // This allows better control over what gets reported. 
        return errors.ApplyBaseline(_reportSettings.Baseline)
            .ApplySuppressions(_reportSettings.Suppressions);
    }

    private IEnumerable<GameVerifier> CreateVerificationSteps(IStarWarsGameEngine engine)
    {
        return _pipelineSettings.VerifiersProvider
            .GetVerifiers(engine, _pipelineSettings.GameVerifySettings, ServiceProvider);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _aggregatedVerifyProgressReporter?.Dispose();
    }
}