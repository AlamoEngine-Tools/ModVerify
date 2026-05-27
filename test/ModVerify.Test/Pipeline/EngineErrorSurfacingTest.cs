using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Verifiers;
using ModVerify.Test.Framework;
using ModVerify.Test.Framework.Providers;
using Xunit;

namespace ModVerify.Test.Pipeline;

public class EngineErrorSurfacingTest : ModVerifyTestBase
{
    [Fact]
    public async Task Verify_MalformedXml_SurfacesAsVerificationError()
    {
        using var repo = CreateBuilder()
            .WithMinimalFoc()
            .WithGame(g => g.WriteXml("SFXEventFiles.xml", "<<not-xml"))
            .Build();

        var result = await RunPipelineAsync(repo, verifiers: new NoVerifiersProvider());

        var xmlErrors = result.NewErrors
            .Where(e => e.Id is VerifierErrorCodes.GenericXmlError or VerifierErrorCodes.EmptyXmlRoot)
            .ToList();
        Assert.NotEmpty(xmlErrors);
    }
}
