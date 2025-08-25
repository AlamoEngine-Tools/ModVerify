using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;

namespace AET.ModVerify.App.Updates.SelfUpdate;


internal class ModVerifyApplicationUpdater : ApplicationUpdater
{
    public ModVerifyApplicationUpdater(
        ModVerifyUpdateMode updateMode,
        UpdatableApplicationEnvironment environment,
        IServiceProvider serviceProvider) 
        : base(environment, serviceProvider)
    {
        if (updateMode == ModVerifyUpdateMode.CheckOnly)
            throw new ArgumentException("Check-only mode is not supported by this updater.", nameof(updateMode));
    }

    public override Task<UpdateCatalog> CheckForUpdateAsync(ProductBranch branch, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public override Task UpdateAsync(UpdateCatalog updateCatalog, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}