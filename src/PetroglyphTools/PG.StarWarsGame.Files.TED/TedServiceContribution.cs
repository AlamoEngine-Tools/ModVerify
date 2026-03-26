using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.TED.Binary.Reader;
using PG.StarWarsGame.Files.TED.Services;

namespace PG.StarWarsGame.Files.TED;

public static class TedServiceContribution
{
    public static void SupportTED(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITedFileReaderFactory>(sp => new TedFileReaderFactory(sp));
        serviceCollection.AddSingleton<ITedFileService>(sp => new TedFileService(sp));
    }
}