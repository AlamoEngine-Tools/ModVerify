using System;
using AET.ModVerifyTool.Options;

namespace AET.ModVerifyTool.ModSelectors;

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