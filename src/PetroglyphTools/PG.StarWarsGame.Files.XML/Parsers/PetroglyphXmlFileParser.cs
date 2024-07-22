﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.Commons.Utilities;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParser<T>(IServiceProvider serviceProvider) : PetroglyphXmlParser<T>(serviceProvider), IPetroglyphXmlFileParser<T>
{
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
        var fileName = xmlStream.GetFilePath();
        if (string.IsNullOrEmpty(fileName))
            throw new InvalidOperationException("Unable to parse XML from unnamed stream. Either parse from a file or MEG stream.");

        var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings(), fileName);

        var options = LoadOptions.SetBaseUri;
        if (LoadLineInfo)
            options |= LoadOptions.SetLineInfo;

        var doc = XDocument.Load(xmlReader, options);
        return doc.Root;
    }

    object? IPetroglyphXmlFileParser.ParseFile(Stream stream)
    {
        return ParseFile(stream);
    }
}