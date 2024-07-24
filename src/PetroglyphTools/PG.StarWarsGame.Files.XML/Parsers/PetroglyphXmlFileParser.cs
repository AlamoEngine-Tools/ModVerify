using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.Commons.Utilities;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParser<T>(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) : 
    PetroglyphXmlParser<T>(serviceProvider, listener), IPetroglyphXmlFileParser<T>
{
    private readonly IXmlParserErrorListener? _listener = listener;
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    protected virtual bool LoadLineInfo => true;

    public T ParseFile(Stream xmlStream)
    {
        var root = GetRootElement(xmlStream);
        return root is null ? default! : Parse(root);
    }

    public void ParseFile(Stream xmlStream, IValueListDictionary<Crc32, T> parsedEntries)
    {
        var root = GetRootElement(xmlStream);
        if (root is not null) 
            Parse(root, parsedEntries);
    }

    protected abstract void Parse(XElement element, IValueListDictionary<Crc32, T> parsedElements);

    private XElement? GetRootElement(Stream xmlStream)
    {
        var fileName = GetStrippedFileName(xmlStream.GetFilePath());
        
        if (string.IsNullOrEmpty(fileName))
            throw new InvalidOperationException("Unable to parse XML from unnamed stream. Either parse from a file or MEG stream.");

        SkipLeadingWhiteSpace(fileName, xmlStream);

        var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        }, fileName);

        var options = LoadOptions.SetBaseUri;
        if (LoadLineInfo)
            options |= LoadOptions.SetLineInfo;

        var doc = XDocument.Load(xmlReader, options);
        return doc.Root;
    }

    private string GetStrippedFileName(string filePath)
    {
        if (!_fileSystem.Path.IsPathFullyQualified(filePath))
            return filePath;

        var pathPartIndex = filePath.LastIndexOf("DATA\\XML\\", StringComparison.OrdinalIgnoreCase);
        
        if (pathPartIndex == -1)
            return filePath;

        return filePath.Substring(pathPartIndex);
    }


    private void SkipLeadingWhiteSpace(string fileName, Stream stream)
    {
        using var r = new StreamReader(stream, Encoding.ASCII, false, 10, true);
        var count = 0;

        while (true)
        {
            var c = (char)r.Read();
            if (!char.IsWhiteSpace(c))
                break;
            count++;
        }

        if (count != 0)
            _listener?.OnXmlParseError(this, new XmlParseErrorEventArgs(fileName, null, 
                XmlParseErrorKind.DataBeforeHeader, $"XML header is not the first entry of the file '{fileName}'"));

        stream.Position = count;
    }


    object? IPetroglyphXmlFileParser.ParseFile(Stream stream)
    {
        return ParseFile(stream);
    }
}