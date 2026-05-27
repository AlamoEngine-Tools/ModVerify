using System.Threading.Tasks;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.Engine;
using ModVerify.Test.Framework;
using Xunit;

namespace ModVerify.Test.Verifiers;

public class HardcodedAssetsVerifierTest : VerifierTestBase<HardcodedAssetsVerifier>
{
    [Fact]
    public async Task Verify_MissingHardcodedAsset_EmitsFileNotFound()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc()
            .Build();

        var errors = await RunAsync(repo,
            (engine, settings, sp) => new HardcodedAssetsVerifier(engine, settings, sp));

        Assert.Contains(errors, e =>
            e.Id == VerifierErrorCodes.FileNotFound &&
            e.Asset == "I_TUTORIAL_ARROW.ALO");
    }
}
