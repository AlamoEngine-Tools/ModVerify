using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine;

public static class PetroglyphEngineServiceContribution
{
    public static void ContributeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameLanguageManager>(sp => new GameLanguageManager(sp));
        serviceCollection.AddSingleton<IPetroglyphXmlFileParserFactory>(sp => new PetroglyphXmlFileParserFactory(sp));
    }
}