using System.Threading;
using AET.ModVerify.Verifiers.Commons;
using AET.ModVerify.Verifiers.Utilities;

namespace AET.ModVerify.Verifiers.SfxEvents;

public partial class SfxEventVerifier
{
    private void VerifyDuplicates(CancellationToken token)
    {
        var context = IDuplicateVerificationContext.CreateForNamedXmlObjects(GameEngine.SfxGameManager, "SFXEvent");
        var verifier = new DuplicateVerifier(this, GameEngine, Settings, Services);
        verifier.Verify(context, [], token);
        foreach (var error in verifier.VerifyErrors)
            AddError(error);
    }
}
