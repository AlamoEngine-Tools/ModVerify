using System;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class XmlContainerContentParser(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IXmlContainerContentParser
{
    public event EventHandler<XmlContainerParserErrorEventArgs>? XmlParseError;

    private readonly IPetroglyphXmlFileParserFactory _fileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();

    public void ParseEntriesFromFileListXml<T>(
        string xmlFile,
        IXmlParserErrorReporter reporter,
        IGameRepository gameRepository,
        string lookupPath,
        ValueListDictionary<Crc32, T> entries,
        Action<string>? onFileParseAction = null) where T : notnull
    {
        Logger.LogDebug($"Parsing container data '{xmlFile}'");

        using var containerStream = gameRepository.TryOpenFile(xmlFile);
        if (containerStream == null)
        {
            reporter.Report(ToString(), XmlParseErrorEventArgs.FromMissingFile(xmlFile));
            Logger.LogWarning($"Could not find XML file '{xmlFile}'");

            var args = new XmlContainerParserErrorEventArgs(xmlFile, null, true)
            {
                // No reason to continue
                Continue = false
            };
            XmlParseError?.Invoke(this, args);
            return;
        }

        XmlFileListContainer? container;

        try
        {
            var containerParser = new XmlFileListParser(Services, reporter);
            container = containerParser.ParseFile(containerStream);
            if (container is null)
                throw new XmlException($"Unable to parse XML container file '{xmlFile}'.");
        }
        catch (XmlException e)
        {
            var args = new XmlContainerParserErrorEventArgs(xmlFile, e, true)
            {
                // No reason to continue
                Continue = false
            };
            XmlParseError?.Invoke(this, args);
            return;
        }



        var xmlFiles = container.Files.Select(x => FileSystem.Path.Combine(lookupPath, x)).ToList();

        var parser = _fileParserFactory.CreateFileParser<T>(reporter);

        foreach (var file in xmlFiles)
        {
            if (onFileParseAction is not null)
                onFileParseAction(file);

            using var fileStream = gameRepository.TryOpenFile(file);

            if (fileStream is null)
            {
                reporter.Report(parser.ToString(), XmlParseErrorEventArgs.FromMissingFile(file));
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
                reporter.Report(parser.ToString(), new XmlParseErrorEventArgs(new XmlLocationInfo(file, 0), XmlParseErrorKind.Unknown, e.Message));

                var args = new XmlContainerParserErrorEventArgs(file, e);
                XmlParseError?.Invoke(this, args);

                if (!args.Continue)
                    return;
            }
        }
    }

    public override string ToString()
    {
        return GetType().FullName;
    }
}