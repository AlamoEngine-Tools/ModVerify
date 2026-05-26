using System;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework.Handlers;
using AnakinRaW.AppUpdaterFramework.Updater;

namespace AET.ModVerify.App.Updates.SelfUpdate;

internal sealed class ModVerifyUpdateResultHandler(
    UpdatableApplicationEnvironment applicationEnvironment,
    IServiceProvider serviceProvider,
    bool restartHostAfterUpdate = true)
    : ApplicationUpdateResultHandler(applicationEnvironment, serviceProvider)
{
    protected override bool RestartHostAfterUpdate => restartHostAfterUpdate;

    protected override Task ShowError(UpdateResult updateResult)
    {
        Console.WriteLine();
        Console.WriteLine($"Update failed with error: {updateResult.ErrorMessage}");
        return base.ShowError(updateResult);
    }

    protected override void RestartApplication(RestartReason reason)
    {
        Console.WriteLine();
        if (reason == RestartReason.Update && !restartHostAfterUpdate)
            Console.WriteLine("Applying update and exiting; the application will not be relaunched.");
        else
            Console.WriteLine("Restarting application to complete update...");
        base.RestartApplication(reason);
    }
}
