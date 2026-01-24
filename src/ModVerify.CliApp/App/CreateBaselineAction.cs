using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Utilities;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace AET.ModVerify.App;

internal sealed class CreateBaselineAction(AppBaselineSettings settings, IServiceProvider serviceProvider)
    : ModVerifyApplicationAction<AppBaselineSettings>(settings, serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    
    protected override void PrintAction(VerificationTarget target)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Creating baseline for {target.Name}...");
        Console.WriteLine();
        ModVerifyConsoleUtilities.WriteSelectedTarget(target);
        Console.WriteLine();
    }

    protected override async Task<int> ProcessVerifyFindings(
        VerificationTarget verificationTarget, 
        IReadOnlyCollection<VerificationError> allErrors)
    {
        var baselineFactory = ServiceProvider.GetRequiredService<IBaselineFactory>();
        var baseline = baselineFactory.CreateBaseline(verificationTarget, Settings, allErrors);

        var fullPath = _fileSystem.Path.GetFullPath(Settings.NewBaselinePath);
        Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, 
            "Writing Baseline to '{FullPath}' with {Number} findings", fullPath, allErrors.Count);

        await baselineFactory.WriteBaselineAsync(baseline, Settings.NewBaselinePath);

        Logger?.LogDebug("Baseline successfully created.");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Baseline for {verificationTarget.Name} created.");
        Console.ResetColor();

        return ModVerifyConstants.Success;
    }

    protected override VerificationBaseline GetBaseline(VerificationTarget verificationTarget)
    {
        return VerificationBaseline.Empty;
    }
}