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
        using var containerStream = gameRepository.OpenFile(xmlFile);
        var containerParser = _fileParserFactory.GetFileParser<XmlFileContainer>(listener);
        Logger.LogDebug($"Parsing container data '{xmlFile}'");
        var container = containerParser.ParseFile(containerStream);

        var xmlFiles = container?.Files.Select(x => FileSystem.Path.Combine("DATA\\XML", x)).ToList();
        if (xmlFiles is null)
            return;

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
                listener.OnXmlParseError(parser, new XmlParseErrorEventArgs(file, null, XmlParseErrorKind.Unknown, e.Message));
            }
        }
    }
}