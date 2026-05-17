using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Testing;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.MEG;
using PG.StarWarsGame.Files.MTD;
using PG.StarWarsGame.Files.XML;
using Testably.Abstractions;
using Xunit;

namespace ModVerify.Test.Framework;

public abstract class ModVerifyTestBase : TestBaseWithFileSystem
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);

        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));

        serviceCollection.SupportMTD();
        serviceCollection.SupportMEG();
        serviceCollection.SupportALO();
        serviceCollection.SupportXML();
        PetroglyphCommons.ContributeServices(serviceCollection);
        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
    }

    protected override IFileSystem CreateFileSystem()
    {
        return new RealFileSystem();
    }

    /// <summary>Creates a builder bound to the test base's <see cref="IFileSystem"/>.</summary>
    protected VirtualGameRepoBuilder CreateBuilder()
    {
        return new VirtualGameRepoBuilder(ServiceProvider.GetRequiredService<IFileSystem>());
    }

    /// <summary>Creates the default <see cref="VerifierServiceSettings"/> for a pipeline run.</summary>
    /// <remarks><see cref="VerifierServiceSettings.ParallelVerifiers"/> defaults to <c>1</c> so the order of verifier invocations is deterministic.</remarks>
    protected virtual VerifierServiceSettings CreateDefaultSettings(IGameVerifiersProvider verifiers)
    {
        return new VerifierServiceSettings
        {
            VerifiersProvider = verifiers,
            ParallelVerifiers = 1,
            UseLiveVirtualFileSystem = false,
            GameVerifySettings = GameVerifySettings.Default,
            FailFastSettings = FailFastSetting.NoFailFast,
        };
    }

    protected async Task<VerificationErrors> RunPipelineAsync(
        VirtualGameRepo repo,
        IGameVerifiersProvider? verifiers = null,
        BaselineCollection? baselines = null,
        SuppressionList? suppressions = null,
        VerifierServiceSettings? settings = null)
    {
        if (repo == null)
            throw new ArgumentNullException(nameof(repo));

        var serviceSettings = settings ?? CreateDefaultSettings(
            verifiers ?? new DefaultGameVerifiersProvider());
        var target = new VerificationTarget
        {
            Engine = GameEngineType.Foc,
            Location = repo.GameLocations,
            Name = "test-target",
        };

        using var pipeline = new GameVerifyPipeline(
            target,
            serviceSettings,
            ServiceProvider,
            baselines ?? BaselineCollection.Empty, 
            suppressions ?? SuppressionList.Empty);
        await pipeline.RunAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        return pipeline.Errors;
    }
}
