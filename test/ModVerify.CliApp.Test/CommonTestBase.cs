using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Hashing;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients.Steam;

namespace ModVerify.CliApp.Test;

public abstract class CommonTestBase : AET.Testing.TestBaseWithFileSystem
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));
        PetroglyphCommons.ContributeServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
        SteamPetroglyphStarWarsGameClients.InitializeServices(serviceCollection);
    }
}