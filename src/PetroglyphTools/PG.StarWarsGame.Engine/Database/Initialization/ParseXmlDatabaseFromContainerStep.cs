using System;
using System.IO.Abstractions;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database.Initialization;

internal class ParseXmlDatabaseFromContainerStep<T>(
    string name,
    string xmlFile,
    IGameRepository repository,
    IXmlParserErrorListener? listener,
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
        var containerParser = FileParserFactory.GetFileParser<XmlFileContainer>(listener);
        Logger?.LogDebug($"Parsing container data '{xmlFile}'");
        var container = containerParser.ParseFile(containerStream);

        var xmlFiles = container.Files.Select(x => _fileSystem.Path.Combine("DATA\\XML", x)).ToList();

        var parsedEntries = new ValueListDictionary<Crc32, T>();

        foreach (var file in xmlFiles)
        {
            using var fileStream = GameRepository.TryOpenFile(file);
            
            var parser = FileParserFactory.GetFileParser<T>(listener);

            if (fileStream is null)
            {
                listener?.OnXmlParseError(parser, XmlParseErrorEventArgs.FromMissingFile(file));
                Logger?.LogWarning($"Could not find XML file '{file}'");
                continue;
            }

            Logger?.LogDebug($"Parsing File '{file}'");

            try
            {
                parser.ParseFile(fileStream, parsedEntries);
            }
            catch (XmlException e)
            {
                listener?.OnXmlParseError(parser, new XmlParseErrorEventArgs(file, null, XmlParseErrorKind.Unknown, e.Message));
            }
        }

        return new XmlDatabase<T>(parsedEntries, Services);
    }
}