using System.Threading.Tasks;
using ModVerify.Test.Framework;
using ModVerify.Test.Framework.Providers;
using Xunit;

namespace ModVerify.Test.Pipeline;

public class MinimalFocTest : ModVerifyTestBase
{
    [Fact]
    public async Task Verify_MinimalFoc_BootsCleanWithoutInitErrors()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc()
            .Build();

        var result = await RunPipelineAsync(repo, verifiers: new NoVerifiersProvider());

        Assert.Empty(result.NewErrors);
        Assert.Empty(result.PersistentErrors.Values);
    }
}
