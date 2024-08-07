﻿using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine;

public static class PetroglyphEngineServiceContribution
{
    public static void ContributeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameRepositoryFactory>(sp => new GameRepositoryFactory(sp));
        serviceCollection.AddSingleton<IGameLanguageManagerProvider>(sp => new GameLanguageManagerProvider(sp));
        serviceCollection.AddSingleton<IPetroglyphXmlFileParserFactory>(sp => new PetroglyphXmlFileParserFactory(sp));
        
        serviceCollection.AddTransient<IGameDatabaseService>(sp => new GameDatabaseService(sp));
    }
}