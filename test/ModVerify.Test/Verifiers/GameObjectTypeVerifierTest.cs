using System.Threading.Tasks;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.GameObjects;
using ModVerify.Test.Framework;
using Xunit;

namespace ModVerify.Test.Verifiers;

public class GameObjectTypeVerifierTest : VerifierTestBase<GameObjectTypeVerifier>
{
    [Fact]
    public async Task Verify_GameObjectWithMissingLandModel_EmitsFileNotFound()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc(ServiceProvider)
            .WithGame(g =>
            {
                g.WriteXml("Empty_GameObjects.xml", """
                    <?xml version="1.0" encoding="utf-8"?>
                    <GameObjectTypes>
                      <Infantry Name="INFANTRY_TROOPER_TEST">
                        <Land_Model_Name>does_not_exist.alo</Land_Model_Name>
                      </Infantry>
                    </GameObjectTypes>
                    """);
            })
            .Build();

        var errors = await RunAsync(repo,
            (engine, settings, sp) => new GameObjectTypeVerifier(engine, settings, sp));

        ErrorAssertions.Single(errors,
            id: VerifierErrorCodes.FileNotFound,
            asset: "DOES_NOT_EXIST.ALO",
            contextContains: "INFANTRY_TROOPER_TEST");
    }
}
