using System;
using AET.ModVerify.App.Settings;

namespace AET.ModVerify.App.ModSelectors;

internal class SettingsBasedVerificationTargetCreator(IServiceProvider serviceProvider)
{
    public VerificationTarget CreateFromSettings(GameInstallationsSettings settings)
    {
        return new VerificationTargetSelectorFactory(serviceProvider)
            .CreateSelector(settings)
            .Select(settings);
    }
}