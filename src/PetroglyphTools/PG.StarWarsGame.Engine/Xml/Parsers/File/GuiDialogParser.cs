﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class GuiDialogParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) : 
    PetroglyphXmlFileParser<GuiDialogsXml>(serviceProvider, listener)
{ 
    protected override GuiDialogsXml Parse(XElement element, string fileName)
    {
        var textures = ParseTextures(element.Element("Textures"), fileName);
        return new GuiDialogsXml(textures);
    }

    private GuiDialogsXmlTextureData ParseTextures(XElement? element, string fileName)
    {
        if (element is null)
        {
            OnParseError(new XmlParseErrorEventArgs(fileName, element, XmlParseErrorKind.MissingNode, "Expected node <Textures> is missing."));
            return new GuiDialogsXmlTextureData([]);
        }

        var textures = new List<XmlComponentTextureData>();

        GetAttributeValue(element, "File", out var megaTexture);
        GetAttributeValue(element, "Compressed_File", out var compressedMegaTexture);

        foreach (var texture in element.Elements()) 
            textures.Add(ParseTexture(texture));

        if (textures.Count == 0)
            OnParseError(new XmlParseErrorEventArgs(fileName, element, XmlParseErrorKind.MissingNode, "Textures must contain at least one child node."));
        
        return new GuiDialogsXmlTextureData(textures)
        {
            MegaTexture = megaTexture,
            CompressedMegaTexture = compressedMegaTexture
        };
    }

    private XmlComponentTextureData ParseTexture(XElement texture)
    {
        var componentId = GetTagName(texture);
        var textures = new ValueListDictionary<string, string>();

        foreach (var entry in texture.Elements()) 
            textures.Add(entry.Name.ToString(), PrimitiveParserProvider.StringParser.Parse(entry));
        
        return new XmlComponentTextureData(
            componentId,
            textures,
            new ValueListDictionary<string, object?>(),
            XmlLocationInfo.FromElement(texture));
    }

    protected override void Parse(XElement element, IValueListDictionary<Crc32, GuiDialogsXml> parsedElements, string fileName)
    {
        throw new NotSupportedException();
    }
}