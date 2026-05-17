using System.Threading.Tasks;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.SfxEvents;
using ModVerify.Test.Framework;
using Xunit;

namespace ModVerify.Test.Verifiers;

public class SfxEventVerifierTest : VerifierTestBase<SfxEventVerifier>
{
    [Fact]
    public async Task Verify_SfxEventWithMissingSample_EmitsFileNotFound()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc(ServiceProvider)
            .WithGame(g =>
            {
                g.WriteXml("Empty_SFXEvents.xml", """
                    <?xml version="1.0" encoding="utf-8"?>
                    <SFXEvents>
                      <SFXEvent Name="TestEvent">
                        <Samples>missing_sample.wav</Samples>
                      </SFXEvent>
                    </SFXEvents>
                    """);
            })
            .Build();

        var errors = await RunAsync(repo,
            (engine, settings, sp) => new SfxEventVerifier(engine, settings, sp));

        ErrorAssertions.Single(errors,
            id: VerifierErrorCodes.FileNotFound,
            asset: "MISSING_SAMPLE.WAV",
            contextContains: "TestEvent");
    }
}
