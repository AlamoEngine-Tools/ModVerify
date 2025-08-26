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
        await Task.Delay(2000, token);
        throw new Exception("Test");
    }

    public override Task UpdateAsync(UpdateCatalog updateCatalog, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}