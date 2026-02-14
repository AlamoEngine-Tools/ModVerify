using System;
using System.Linq;
using System.Xml;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class XmlContainerContentParser : ServiceBase, IPetroglyphXmlParser
{
    public event EventHandler<XmlContainerParserErrorEventArgs>? XmlParseError;

    private readonly IXmlParserErrorReporter? _reporter;
    private readonly IPetroglyphXmlFileParserFactory _fileParserFactory;

    public XmlContainerContentParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? reporter) : base(serviceProvider)
    {
        _reporter = reporter;
        _fileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();
        Name = GetType().FullName!;
    }

    public string Name { get; }

    public void ParseEntriesFromFileListXml<T>(
        string xmlFile,
        IGameRepository gameRepository,
        string lookupPath,
        FrugalValueListDictionary<Crc32, T> entries,
        Action<string>? onFileParseAction = null) where T : notnull
    {
        Logger.LogDebug("Parsing container data '{XmlFile}'", xmlFile);

        using var containerStream = gameRepository.TryOpenFile(xmlFile);
        if (containerStream == null)
        {
            _reporter?.Report(this, XmlParseErrorEventArgs.FromMissingFile(xmlFile));
            Logger.LogWarning("Could not find XML file '{XmlFile}'", xmlFile);

            var args = new XmlContainerParserErrorEventArgs(xmlFile, isXmlFileList: true)
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
            var containerParser = new XmlFileListParser(Services, _reporter);
            container = containerParser.ParseFile(containerStream);
            if (container is null)
                throw new XmlException($"Unable to parse XML container file '{xmlFile}'.");
        }
        catch (XmlException e)
        {
            _reporter?.Report(this, 
                new XmlParseErrorEventArgs(new XmlLocationInfo(xmlFile, e.LineNumber), XmlParseErrorKind.Unknown, e.Message));

            var args = new XmlContainerParserErrorEventArgs(xmlFile, e, isXmlFileList: true)
            {
                // No reason to continue
                Continue = false
            };
            XmlParseError?.Invoke(this, args);
            return;
        }


        var xmlFiles = container.Files.Select(x => FileSystem.Path.Combine(lookupPath, x)).ToList();

        var parser = _fileParserFactory.CreateFileParser<T>(_reporter);
        
        foreach (var file in xmlFiles)
        {
            if (onFileParseAction is not null)
                onFileParseAction(file);

            using var fileStream = gameRepository.TryOpenFile(file);

            if (fileStream is null)
            {
                _reporter?.Report(parser, XmlParseErrorEventArgs.FromMissingFile(file));
                Logger.LogWarning("Could not find XML file '{File}'", file);

                var args = new XmlContainerParserErrorEventArgs(file, isXmlFileList: false);
                XmlParseError?.Invoke(this, args);

                if (args.Continue)
                    continue;
                return;
            }

            Logger.LogDebug("Parsing File '{File}'", file);

            try
            {
                parser.ParseFile(fileStream, entries);
            }
            catch (XmlException e)
            {
                _reporter?.Report(parser, new XmlParseErrorEventArgs(new XmlLocationInfo(file, 0), XmlParseErrorKind.Unknown, e.Message));

                var args = new XmlContainerParserErrorEventArgs(file, e, isXmlFileList: false);
                XmlParseError?.Invoke(this, args);

                if (!args.Continue)
                    return;
            }
        }
    }
}