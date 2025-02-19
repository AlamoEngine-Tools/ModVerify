using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine;

public static class PetroglyphEngineServiceContribution
{
    public static void ContributeServices(IServiceCollection serviceCollection)
    {
        // Singletons
        serviceCollection.AddSingleton<IGameRepositoryFactory>(sp => new GameRepositoryFactory(sp));
        serviceCollection.AddSingleton<IGameLanguageManagerProvider>(sp => new GameLanguageManagerProvider(sp));
        serviceCollection.AddSingleton<IPetroglyphXmlFileParserFactory>(sp => new PetroglyphXmlFileParserFactory(sp));
        
        serviceCollection.AddSingleton<IGameDatabaseService>(sp => new GameDatabaseService(sp));
    }
}