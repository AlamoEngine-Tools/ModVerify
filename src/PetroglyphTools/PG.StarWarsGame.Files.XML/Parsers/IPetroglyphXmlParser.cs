using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlParser
{
    event EventHandler<ParseErrorEventArgs> ParseError; 

    object Parse(XElement element);
}

public interface IPetroglyphXmlParser<T> : IPetroglyphXmlParser
{
    new T Parse(XElement element);
}