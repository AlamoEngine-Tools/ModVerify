using System;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class XmlContainerContentParser(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IXmlContainerContentParser
{
    public event EventHandler<XmlContainerParserErrorEventArgs>? XmlParseError;

    private readonly IPetroglyphXmlFileParserFactory _fileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

    public void ParseEntriesFromContainerXml<T>(
        string xmlFile,
        IXmlParserErrorListener listener,
        IGameRepository gameRepository,
        string lookupPath,
        ValueListDictionary<Crc32, T> entries,
        Action<string>? onFileParseAction = null)
    {
        var containerParser = _fileParserFactory.CreateFileParser<XmlFileContainer?>(listener);
        Logger.LogDebug($"Parsing container data '{xmlFile}'");

        using var containerStream = gameRepository.TryOpenFile(xmlFile);
        if (containerStream == null)
        {
            listener.OnXmlParseError(containerParser, XmlParseErrorEventArgs.FromMissingFile(xmlFile));
            Logger.LogWarning($"Could not find XML file '{xmlFile}'");

            var args = new XmlContainerParserErrorEventArgs(xmlFile, true)
            {
                // No reason to continue
                Continue = false
            };
            XmlParseError?.Invoke(this, args);
            return;
        }

        XmlFileContainer? container;
        try
        {
            container = containerParser.ParseFile(containerStream);
            if (container is null)
                throw new XmlException($"Unable to parse XML container file '{xmlFile}'.");
        }
        catch (XmlException e)
        {
            var args = new XmlContainerParserErrorEventArgs(e, xmlFile, true)
            {
                // No reason to continue
                Continue = false
            };
            XmlParseError?.Invoke(this, args);
            return;
        }



        var xmlFiles = container.Files.Select(x => FileSystem.Path.Combine(lookupPath, x)).ToList();

        var parser = _fileParserFactory.CreateFileParser<T>(listener);

        foreach (var file in xmlFiles)
        {
            if (onFileParseAction is not null)
                onFileParseAction(file);

            using var fileStream = gameRepository.TryOpenFile(file);

            if (fileStream is null)
            {
                listener.OnXmlParseError(parser, XmlParseErrorEventArgs.FromMissingFile(file));
                Logger.LogWarning($"Could not find XML file '{file}'");

                var args = new XmlContainerParserErrorEventArgs(file);
                XmlParseError?.Invoke(this, args);

                if (args.Continue)
                    continue;
                return;
            }

            Logger.LogDebug($"Parsing File '{file}'");

            try
            {
                parser.ParseFile(fileStream, entries);
            }
            catch (XmlException e)
            {
                listener.OnXmlParseError(parser, new XmlParseErrorEventArgs(new XmlLocationInfo(file, 0), XmlParseErrorKind.Unknown, e.Message));

                var args = new XmlContainerParserErrorEventArgs(e, file);
                XmlParseError?.Invoke(this, args);

                if (!args.Continue)
                    return;
            }
        }
    }
}