using System;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework.Handlers;
using AnakinRaW.AppUpdaterFramework.Updater;

namespace AET.ModVerify.App.Updates.SelfUpdate;

internal sealed class ModVerifyUpdateResultHandler(
    UpdatableApplicationEnvironment applicationEnvironment,
    IServiceProvider serviceProvider)
    : ApplicationUpdateResultHandler(applicationEnvironment, serviceProvider)
{
    protected override Task ShowError(UpdateResult updateResult)
    {
        Console.WriteLine();
        Console.WriteLine($"Update failed with error: {updateResult.ErrorMessage}");
        return base.ShowError(updateResult);
    }

    protected override void RestartApplication(RestartReason reason)
    {
        Console.WriteLine();
        Console.WriteLine("Restarting application to complete update...");
        base.RestartApplication(reason);
    }
}