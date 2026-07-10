using AnakinRaW.CommonUtilities.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Files.TED.Test;

public class CommonDatTestBase : TestBaseWithFileSystem
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.SupportTED();
    }
}