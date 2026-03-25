using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Progress;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Engine;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Settings;
using AET.ModVerify.Utilities;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.Engine;
using AET.ModVerify.Verifiers.Utilities;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

internal sealed class GameVerifyPipeline : StepRunnerPipelineBase<AsyncStepRunner>
{
    private readonly List<GameVerifier> _verifiers = [];
    private readonly List<VerificationError> _errors = [];
    private readonly List<GameVerifierPipelineStep> _verificationSteps = [];
    
    private readonly GameEngineErrorCollection _engineErrorReporter = new();
    private readonly VerificationTarget _verificationTarget;
    private readonly VerifierServiceSettings _serviceSettings;
    private readonly IVerifyProgressReporter? _progressReporter;
    private readonly IGameEngineInitializationReporter? _engineInitializationReporter;
    private readonly VerificationBaseline _baseline;
    private readonly SuppressionList _suppressions;

    internal IReadOnlyCollection<VerificationError> Errors => [.._errors];

    internal IReadOnlyCollection<IGameVerifierInfo> Verifiers => [.. _verifiers];

    private AggregatedVerifyProgressReporter? _aggregatedVerifyProgressReporter;

    public GameVerifyPipeline(
        VerificationTarget verificationTarget,
        VerifierServiceSettings serviceSettings,
        IServiceProvider serviceProvider,
        VerificationBaseline baseline,
        SuppressionList suppressions,
        IVerifyProgressReporter? progressReporter = null,
        IGameEngineInitializationReporter? engineInitializationReporter = null)
        : base(serviceProvider)
    {
        _verificationTarget = verificationTarget ?? throw new ArgumentNullException(nameof(verificationTarget));
        _serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _suppressions = suppressions ?? throw new ArgumentNullException(nameof(suppressions));
        _progressReporter = progressReporter;
        _engineInitializationReporter = engineInitializationReporter;

        FailFast = serviceSettings.FailFastSettings.IsFailFast;
    }
    
    protected override AsyncStepRunner CreateRunner()
    {
        var requestedRunnerCount = _serviceSettings.ParallelVerifiers;
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
        _errors.Clear();

        IStarWarsGameEngine gameEngine;

        try
        {
            var engineService = ServiceProvider.GetRequiredService<IPetroglyphStarWarsGameEngineService>();
            gameEngine = await engineService.InitializeAsync(
                _verificationTarget.Engine,
                _verificationTarget.Location,
                _engineErrorReporter,
                _engineInitializationReporter,
                false,
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger?.LogError(e, "Creating game engine failed: {Message}", e.Message);
            throw;
        }

        AddStep(new GameEngineErrorCollector(_engineErrorReporter, gameEngine, _serviceSettings.GameVerifySettings, ServiceProvider));

        foreach (var gameVerificationStep in CreateVerifiers(gameEngine))
            AddStep(gameVerificationStep);
    }

    protected override void OnExecuteStarted()
    {
        Logger?.LogInformation("Running game verifiers...");
        if (_progressReporter is not null)
        {
            _aggregatedVerifyProgressReporter = new AggregatedVerifyProgressReporter(_progressReporter, _verificationSteps);
            _progressReporter.Report(0.0, $"Verifying {_verificationTarget.Name}...", 
                VerifyProgress.ProgressType, default);
        }
    }
    
    protected override void OnExecuteCompleted()
    {
        Logger?.LogInformation("Game verifiers finished.");
        _errors.AddRange(GetReportableErrors(_verifiers.SelectMany(s => s.VerifyErrors)));
        _progressReporter?.Report(1.0, $"Finished Verifying {_verificationTarget.Name}", 
            VerifyProgress.ProgressType, default);
    }

    protected override void OnRunnerExecutionError(object sender, StepRunnerErrorEventArgs e)
    {
        if (FailFast && e.Exception is GameVerificationException verificationException)
        {
            var minSeverity = _serviceSettings.FailFastSettings.MinumumSeverity;
            var ignoreError = verificationException.Errors
                .Where(error => error.Severity >= minSeverity)
                .All(error => _baseline.Contains(error) || _suppressions.Suppresses(error));
            if (ignoreError)
                return;
        }
        base.OnRunnerExecutionError(sender, e);
    }

    protected override IEnumerable<IStep> GetFailedSteps(IEnumerable<IStep> steps)
    {
        return base.GetFailedSteps(steps).Where(s => s.Error is not GameVerificationException);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        _engineErrorReporter.Clear();
        _aggregatedVerifyProgressReporter?.Dispose();
        _aggregatedVerifyProgressReporter = null;
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
        return errors.ApplyBaseline(_baseline).ApplySuppressions(_suppressions);
    }

    private IEnumerable<GameVerifier> CreateVerifiers(IStarWarsGameEngine engine)
    {
        return _serviceSettings.VerifiersProvider
            .GetVerifiers(engine, _serviceSettings.GameVerifySettings, ServiceProvider)
            .Distinct(NameBasedEqualityComparer.Instance);
    }
}