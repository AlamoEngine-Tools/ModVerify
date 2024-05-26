using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using PG.Commons.Utilities;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParser<T>(IServiceProvider serviceProvider) : PetroglyphXmlElementParser<T>(serviceProvider), IPetroglyphXmlFileParser<T>
{
    protected virtual bool LoadLineInfo => false;
    public T ParseFile(Stream xmlStream)
    {
        var fileName = xmlStream.GetFilePath();
        var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings(), fileName);

        var options = LoadOptions.SetBaseUri;
        if (LoadLineInfo)
            options |= LoadOptions.SetLineInfo;

        var doc = XDocument.Load(xmlReader, options);
        var root = doc.Root;
        if (root is null)
            return default!;
        return Parse(root);
    }

    object? IPetroglyphXmlFileParser.ParseFile(Stream stream)
    {
        return ParseFile(stream);
    }
}