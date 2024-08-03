using System;
using ModVerify.CliApp.Options;

namespace ModVerify.CliApp.ModSelectors;

internal class ModSelectorFactory(IServiceProvider serviceProvider)
{
    public IModSelector CreateSelector(GameInstallationsSettings settings)
    {
        if (settings.Interactive)
            return new ConsoleModSelector(serviceProvider);
        if (settings.UseAutoDetection)
            return new AutomaticModSelector(serviceProvider);
        if (settings.ManualSetup)
            return new ManualModSelector(serviceProvider);
        throw new ArgumentException("Unknown option configuration provided.");
    }
}