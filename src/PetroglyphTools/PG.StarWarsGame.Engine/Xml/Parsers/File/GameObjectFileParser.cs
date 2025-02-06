using System;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class GameObjectFileParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) 
    : PetroglyphXmlFileContainerParser<GameObject>(serviceProvider, errorReporter)
{
    protected override void Parse(XElement element, IValueListDictionary<Crc32, GameObject> parsedElements, string fileName)
    {
        var parser = new GameObjectParser(parsedElements, ServiceProvider, ErrorReporter);

        foreach (var xElement in element.Elements())
        {
            var gameObject = parser.Parse(xElement, out var nameCrc);
            parsedElements.Add(nameCrc, gameObject);
        }
    }
}