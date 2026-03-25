using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.Audio.Sfx;

namespace AET.ModVerify.Verifiers.SfxEvents;

public partial class SfxEventVerifier
{
    private void VerifyPresetRef(SfxEvent sfxEvent, string[] context)
    {
        if (!string.IsNullOrEmpty(sfxEvent.UsePresetName) && sfxEvent.Preset is null)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.MissingXRef,
                $"Missing preset '{sfxEvent.UsePresetName}' for SFXEvent '{sfxEvent.Name}'.",
                VerificationSeverity.Error,
                [..context, "Preset"],
                sfxEvent.UsePresetName));
        }
    }
}
