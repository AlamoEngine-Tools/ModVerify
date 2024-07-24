using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database.Initialization;

internal abstract class ParseXmlDatabaseStep<T>(
    IList<string> xmlFiles,
    IGameRepository repository,
    IXmlParserErrorListener? listener,
    IServiceProvider serviceProvider)
    : CreateDatabaseStep<T>(repository, serviceProvider)
    where T : class
{
    protected readonly IPetroglyphXmlFileParserFactory FileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

    protected ParseXmlDatabaseStep(string xmlFile, IGameRepository repository, IXmlParserErrorListener? listener, IServiceProvider serviceProvider)
        : this([xmlFile], repository, listener, serviceProvider)
    {
    }

    protected sealed override T CreateDatabase()
    {
        var parsedDatabaseEntries = new List<T>();
        foreach (var xmlFile in xmlFiles)
        {
            using var fileStream = GameRepository.OpenFile(xmlFile);

            var parser = FileParserFactory.GetFileParser<T>(listener);
            Logger?.LogDebug($"Parsing File '{xmlFile}'");
            var parsedData = parser.ParseFile(fileStream)!;
            parsedDatabaseEntries.Add(parsedData);
        }
        return CreateDatabase(parsedDatabaseEntries);
    }

    protected abstract T CreateDatabase(IList<T> parsedDatabaseEntries);
}