using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.MEG;
using PG.StarWarsGame.Files.MTD;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Represents a base class for engine-bound tests, providing the necessary service
/// registrations for constructing game repositories and related components.
/// </summary>
public abstract class EngineTestBase : TestBaseWithFileSystem
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));

        serviceCollection.SupportMTD();
        serviceCollection.SupportMEG();
        serviceCollection.SupportALO();
        serviceCollection.SupportXML();
        PetroglyphCommons.ContributeServices(serviceCollection);
        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
    }
}