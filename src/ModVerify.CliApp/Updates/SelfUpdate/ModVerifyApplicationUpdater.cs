using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;

namespace AET.ModVerify.App.Updates.SelfUpdate;


internal class ModVerifyApplicationUpdater(
    UpdatableApplicationEnvironment environment,
    IServiceProvider serviceProvider)
    : ApplicationUpdater(environment, serviceProvider)
{
    public override async Task<UpdateCatalog> CheckForUpdateAsync(ProductBranch branch, CancellationToken token = default)
    {
        var updateReference = ProductService.CreateProductReference(null, branch);

        var updateCatalog = await UpdateService.CheckForUpdatesAsync(updateReference, token);

        if (updateCatalog is null)
            throw new InvalidOperationException("Update service was already doing something.");

        return updateCatalog.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall
            ? throw new NotSupportedException("Install and Uninstall operations are not supported")
            : updateCatalog;
    }

    public override Task UpdateAsync(UpdateCatalog updateCatalog, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}