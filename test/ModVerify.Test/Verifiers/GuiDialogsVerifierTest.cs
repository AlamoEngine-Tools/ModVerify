using System.Threading.Tasks;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.GuiDialogs;
using ModVerify.Test.Framework;
using Xunit;

namespace ModVerify.Test.Verifiers;

public class GuiDialogsVerifierTest : VerifierTestBase<GuiDialogsVerifier>
{
    [Fact]
    public async Task Verify_MissingMtdFile_EmitsFileNotFound()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc(ServiceProvider)
            .Build();

        var errors = await RunAsync(repo,
            (engine, settings, sp) => new GuiDialogsVerifier(engine, settings, sp));

        ErrorAssertions.Single(errors,
            id: VerifierErrorCodes.FileNotFound,
            asset: "empty");
    }
}
