using System;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Settings;
using ModVerify.Test.Framework;
using ModVerify.Test.Framework.Providers;
using Xunit;

namespace ModVerify.Test.Pipeline;

public class FailFastTest : ModVerifyTestBase
{
    [Fact]
    public async Task RunPipeline_ErrorAboveThreshold_AbortsBeforeSubsequentVerifiers()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var provider = ErrorThenTrackingProvider.Create(
            id: "TEST00", asset: "asset-1", context: ["ctx"],
            severity: VerificationSeverity.Error);

        var settings = BuildFailFastSettings(provider);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => RunPipelineAsync(repo, settings: settings));

        Assert.False(provider.Tracker!.WasInvoked,
            "TrackingVerifier ran despite fail-fast — pipeline did not actually short-circuit.");
    }

    [Fact]
    public async Task RunPipeline_SuppressedErrorAboveThreshold_DoesNotAbort()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var provider = ErrorThenTrackingProvider.Create(
            id: "TEST00", asset: "asset-1", context: ["ctx"],
            severity: VerificationSeverity.Error);

        var suppressions = new SuppressionList(
        [
            new SuppressionFilter(id: "TEST00", verifier: null, asset: "asset-1"),
        ]);

        var settings = BuildFailFastSettings(provider);

        var result = await RunPipelineAsync(repo, suppressions: suppressions, settings: settings);

        Assert.True(provider.Tracker!.WasInvoked,
            "TrackingVerifier did not run despite the error being suppressed.");
        Assert.Empty(result.NewErrors);
    }

    private static VerifierServiceSettings BuildFailFastSettings(IGameVerifiersProvider provider)
    {
        return new VerifierServiceSettings
        {
            VerifiersProvider = provider,
            ParallelVerifiers = 1,
            UseLiveVirtualFileSystem = false,
            FailFastSettings = new FailFastSetting(VerificationSeverity.Error),
            GameVerifySettings = GameVerifySettings.Default with
            {
                ThrowsOnMinimumSeverity = VerificationSeverity.Error,
            },
        };
    }
}
