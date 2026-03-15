using System.Threading;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.Audio.Sfx;

namespace AET.ModVerify.Verifiers.SfxEvents;

public partial class SfxEventVerifier
{
    private void VerifyPresetRef(SfxEvent sfxEvent, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(sfxEvent.UsePresetName) && sfxEvent.Preset is null)
        {
            AddError(VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes.MissingXRef,
                $"SFX Event '{sfxEvent.Name}' has Use_Preset set to '{sfxEvent.UsePresetName}' but the preset could not be found.",
                VerificationSeverity.Error,
                [sfxEvent.Name],
                sfxEvent.UsePresetName));
        }
    }
}
