using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine;

public static class PetroglyphEngineServiceContribution
{
    public static void ContributeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPetroglyphXmlFileParserFactory>(sp => new PetroglyphXmlFileParserFactory(sp));
    }
}