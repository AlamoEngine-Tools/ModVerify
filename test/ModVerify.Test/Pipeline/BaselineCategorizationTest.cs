using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Verifiers;
using ModVerify.Test.Framework;
using ModVerify.Test.Framework.Providers;
using ModVerify.Test.Framework.Verifiers;
using Xunit;

namespace ModVerify.Test.Pipeline;

public class BaselineCategorizationTest : ModVerifyTestBase
{
    [Fact]
    public async Task Categorize_ErrorInRunAndBaseline_IsExisting()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var baseline = BuildBaseline(("TEST00", "asset-1", ["ctx"]));
        var provider = StaticErrorProvider.Create(
            id: "TEST00", asset: "asset-1", context: ["ctx"]);

        var result = await RunPipelineAsync(repo, verifiers: provider, baselines: baseline);

        Assert.Empty(result.NewErrors);
        Assert.Single(result.ExistingErrors.Values.SelectMany(v => v));
    }

    [Fact]
    public async Task Categorize_ErrorOnlyInRun_IsNew()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();
        var provider = StaticErrorProvider.Create(
            id: "TEST00", asset: "asset-1", context: ["ctx"]);

        var result = await RunPipelineAsync(repo, verifiers: provider);

        Assert.Single(result.NewErrors, e => e is { Id: "TEST00", Asset: "asset-1" });
    }

    [Fact]
    public async Task Categorize_ErrorOnlyInBaseline_IsResolved()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();
        var baseline = BuildBaseline(("TEST00", "asset-1", ["ctx"]));

        var result = await RunPipelineAsync(repo, verifiers: new NoVerifiersProvider(), baselines: baseline);

        Assert.Single(result.ResolvedErrors.Values.SelectMany(v => v),
            e => e is { Id: "TEST00", Asset: "asset-1" });
        Assert.Empty(result.NewErrors);
    }

    [Fact]
    public async Task Categorize_SuppressedErrorAlsoInBaseline_IsResolved()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var baseline = BuildBaseline(("TEST00", "asset-1", ["ctx"]));
        var suppressions = new SuppressionList(
        [
            new SuppressionFilter(id: "TEST00", verifier: null, asset: "asset-1"),
        ]);
        var provider = StaticErrorProvider.Create(
            id: "TEST00", asset: "asset-1", context: ["ctx"]);

        var result = await RunPipelineAsync(repo, verifiers: provider, baselines: baseline, suppressions: suppressions);

        Assert.Empty(result.NewErrors);
        Assert.Empty(result.ExistingErrors.Values.SelectMany(v => v));
        Assert.Single(result.ResolvedErrors.Values.SelectMany(v => v),
            e => e is { Id: "TEST00", Asset: "asset-1" });
    }

    [Fact]
    public async Task Categorize_OneOfTwoEmittedErrorsSuppressed_BothInBaseline_SuppressedIsResolved_OtherIsExisting()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var baseline = BuildBaseline(
            ("TEST00", "asset-1", ["ctx"]),
            ("TEST01", "asset-2", ["ctx"]));
        var suppressions = new SuppressionList(
        [
            new SuppressionFilter(id: "TEST00", verifier: null, asset: "asset-1"),
        ]);
        var provider = StaticErrorProvider.Create(
            new StaticErrorSpec("TEST00", "asset-1", ["ctx"]),
            new StaticErrorSpec("TEST01", "asset-2", ["ctx"]));

        var result = await RunPipelineAsync(repo, verifiers: provider, baselines: baseline, suppressions: suppressions);

        Assert.Empty(result.NewErrors);
        Assert.Single(result.ExistingErrors.Values.SelectMany(v => v),
            e => e is { Id: "TEST01", Asset: "asset-2" });
        Assert.Single(result.ResolvedErrors.Values.SelectMany(v => v),
            e => e is { Id: "TEST00", Asset: "asset-1" });
    }

    [Fact]
    public async Task Categorize_NewErrorAndSuppressedError_OnlyUnsuppressedAppearsAsNew()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var suppressions = new SuppressionList(
        [
            new SuppressionFilter(id: "TEST00", verifier: null, asset: "asset-1"),
        ]);
        var provider = StaticErrorProvider.Create(
            new StaticErrorSpec("TEST00", "asset-1", ["ctx"]),
            new StaticErrorSpec("TEST01", "asset-2", ["ctx"]));

        var result = await RunPipelineAsync(repo, verifiers: provider, suppressions: suppressions);

        Assert.Single(result.NewErrors, e => e is { Id: "TEST01", Asset: "asset-2" });
        Assert.DoesNotContain(result.NewErrors, e => e.Id == "TEST00");
    }

    private static BaselineCollection BuildBaseline(params (string Id, string Asset, string[] Context)[] entries)
    {
        var verifierInfo = new StubVerifierInfo("stub");
        var errors = entries
            .Select(e => new VerificationError(
                e.Id, "baseline error", verifierInfo, e.Context, e.Asset, VerificationSeverity.Warning))
            .ToList();
        var baseline = new VerificationBaseline(VerificationSeverity.Information, errors, target: null);
        return new BaselineCollection([
            new IdentifiedBaseline("test-baseline", baseline, BaselineSource.File)
        ]);
    }

    private sealed class StubVerifierInfo(string name) : IGameVerifierInfo
    {
        public IGameVerifierInfo? Parent => null;
        public IReadOnlyList<IGameVerifierInfo> VerifierChain => [this];
        public string Name { get; } = name;
        public string FriendlyName => Name;
    }
}
