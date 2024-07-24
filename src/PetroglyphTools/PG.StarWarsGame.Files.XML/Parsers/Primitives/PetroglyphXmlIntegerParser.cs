using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlIntegerParser : PetroglyphXmlPrimitiveElementParser<int>
{ 
    internal PetroglyphXmlIntegerParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override int Parse(XElement element)
    {
        // The engines uses the C++ function std::atoi which is a little more loose.
        // For example the value '123d' get parsed to 123,
        // whereas in C# int.TryParse returns (false, 0)

        if (!int.TryParse(element.Value, out var i))
        {
            var location = XmlLocationInfo.FromElement(element);
            OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.MalformedValue,
                $"Expected integer but got '{element.Value}' at {location}"));
            return 0;
        }

        return i;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}