using System.Threading.Tasks;
using AET.ModVerify.Reporting.Suppressions;
using ModVerify.Test.Framework;
using ModVerify.Test.Framework.Providers;
using Xunit;

namespace ModVerify.Test.Pipeline;

public class SuppressionFilteringTest : ModVerifyTestBase
{
    [Fact]
    public async Task RunPipeline_SuppressedError_IsFilteredBeforeBaselineCategorization()
    {
        using var repo = CreateBuilder().WithMinimalFoc().Build();

        var suppressions = new SuppressionList(
        [
            new SuppressionFilter(id: "TEST00", verifier: null, asset: "asset-1"),
        ]);

        var provider = StaticErrorProvider.Create(id: "TEST00", asset: "asset-1", context: ["ctx"]);

        var result = await RunPipelineAsync(repo, verifiers: provider, suppressions: suppressions);

        Assert.Empty(result.NewErrors);
        Assert.Empty(result.ExistingErrors.Values);
    }
}
