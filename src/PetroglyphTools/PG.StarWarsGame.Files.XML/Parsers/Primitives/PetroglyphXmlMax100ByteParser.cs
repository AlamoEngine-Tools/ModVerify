﻿using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlMax100ByteParser : PetroglyphXmlPrimitiveElementParser<byte>
{
    internal PetroglyphXmlMax100ByteParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {

    }

    public override byte Parse(XElement element)
    {
        var intValue = PrimitiveParserProvider.IntParser.Parse(element);

        if (intValue > 100)
            intValue = 100;

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
            var location = XmlLocationInfo.FromElement(element);

            OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.InvalidValue,
                $"Expected a byte value (0 - 255) but got value '{intValue}' at {location}"));
        }

        // Add additional check, cause the PG implementation is broken, but we need to stay "bug-compatible".
        if (asByte > 100)
        {
            var location = XmlLocationInfo.FromElement(element);
            OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.InvalidValue,
                $"Expected a byte value (0 - 100) but got value '{asByte}' at {location}"));
        }

        return asByte;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}