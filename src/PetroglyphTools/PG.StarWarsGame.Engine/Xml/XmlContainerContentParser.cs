using System;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml;

internal class XmlContainerContentParser(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IXmlContainerContentParser
{
    private readonly IPetroglyphXmlFileParserFactory _fileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

    public void ParseEntriesFromContainerXml<T>(
        string xmlFile,
        IXmlParserErrorListener listener,
        IGameRepository gameRepository,
        ValueListDictionary<Crc32, T> entries)
    {
        var containerParser = _fileParserFactory.GetFileParser<XmlFileContainer?>(listener);
        Logger.LogDebug($"Parsing container data '{xmlFile}'");

        using var containerStream = gameRepository.TryOpenFile(xmlFile);
        if (containerStream == null)
        {
            listener.OnXmlParseError(containerParser, XmlParseErrorEventArgs.FromMissingFile(xmlFile));
            Logger.LogWarning($"Could not find XML file '{xmlFile}'");
            return;
        }

        var container = containerParser.ParseFile(containerStream);
        if (container is null)
            return;

        var xmlFiles = container.Files.Select(x => FileSystem.Path.Combine("DATA\\XML", x)).ToList();

        var parser = _fileParserFactory.GetFileParser<T>(listener);

        foreach (var file in xmlFiles)
        {
            using var fileStream = gameRepository.TryOpenFile(file);

            if (fileStream is null)
            {
                listener.OnXmlParseError(parser, XmlParseErrorEventArgs.FromMissingFile(file));
                Logger.LogWarning($"Could not find XML file '{file}'");
                continue;
            }

            Logger.LogDebug($"Parsing File '{file}'");

            try
            {
                parser.ParseFile(fileStream, entries);
            }
            catch (XmlException e)
            {
                listener.OnXmlParseError(parser, new XmlParseErrorEventArgs(new XmlLocationInfo(file, 0), XmlParseErrorKind.Unknown, e.Message));
            }
        }
    }
}