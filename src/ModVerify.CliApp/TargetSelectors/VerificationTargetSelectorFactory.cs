using System;
using AET.ModVerify.App.Settings;

namespace AET.ModVerify.App.TargetSelectors;

internal sealed class VerificationTargetSelectorFactory(IServiceProvider serviceProvider)
{
    public IVerificationTargetSelector CreateSelector(VerificationTargetSettings settings)
    {
        if (settings.Interactive)
            return new ConsoleSelector(serviceProvider);
        if (settings.UseAutoDetection)
            return new AutomaticSelector(serviceProvider);
        if (settings.ManualSetup)
            return new ManualSelector(serviceProvider);
        throw new ArgumentException("Unknown option configuration provided.");
    }
}