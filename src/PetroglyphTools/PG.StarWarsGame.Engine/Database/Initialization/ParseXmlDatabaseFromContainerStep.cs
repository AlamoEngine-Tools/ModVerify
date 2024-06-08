using System;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Database.Initialization;

internal class ParseXmlDatabaseFromContainerStep<T>(
    string name,
    string xmlFile,
    IGameRepository repository,
    IServiceProvider serviceProvider)
    : CreateDatabaseStep<IXmlDatabase<T>>(repository, serviceProvider)
    where T : XmlObject
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    protected readonly IPetroglyphXmlFileParserFactory FileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

    protected override string Name => name;

    protected sealed override IXmlDatabase<T> CreateDatabase()
    {
        using var containerStream = GameRepository.OpenFile(xmlFile);
        var containerParser = FileParserFactory.GetFileParser<XmlFileContainer>();
        Logger?.LogDebug($"Parsing container data '{xmlFile}'");
        var container = containerParser.ParseFile(containerStream);

        var xmlFiles = container.Files.Select(x => _fileSystem.Path.Combine("DATA\\XML", x)).ToList();

        var parsedEntries = new ValueListDictionary<Crc32, T>();

        foreach (var file in xmlFiles)
        {
            using var fileStream = GameRepository.OpenFile(file);

            var parser = FileParserFactory.GetFileParser<T>();
            Logger?.LogDebug($"Parsing File '{file}'");
            parser.ParseFile(fileStream, parsedEntries);
        }

        return new XmlDatabase<T>(parsedEntries, Services);
    }
}