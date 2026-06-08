using AET.ModVerify.Reporting.Diagnostics;
using PG.StarWarsGame.Engine.Audio.Sfx;

namespace AET.ModVerify.Verifiers.SfxEvents;

public partial class SfxEventVerifier
{
    private void VerifyPresetRef(SfxEvent sfxEvent, string[] context)
    {
        if (!string.IsNullOrEmpty(sfxEvent.UsePresetName) && sfxEvent.Preset is null)
        {
            AddError(SfxErrors.MissingPreset(this, sfxEvent.UsePresetName!, sfxEvent.Name, [..context, "Preset"]));
        }
    }
}
