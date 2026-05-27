using System.Threading.Tasks;
using AET.ModVerify.Verifiers.CommandBar;
using ModVerify.Test.Framework;
using Xunit;

namespace ModVerify.Test.Verifiers;

public class CommandBarVerifierTest : VerifierTestBase<CommandBarVerifier>
{
    [Fact]
    public async Task Verify_NoShellsGroup_EmitsCmdBarError()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc()
            .Build();

        var errors = await RunAsync(repo,
            (engine, settings, sp) => new CommandBarVerifier(engine, settings, sp));

        ErrorAssertions.Single(errors,
            id: CommandBarVerifier.CommandBarNoShellsGroup,
            asset: "GameCommandBar");
    }
}
