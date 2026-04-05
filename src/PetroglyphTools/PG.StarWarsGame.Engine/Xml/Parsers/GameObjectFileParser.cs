using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.IO;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class GameObjectFileParser(
    GameEngineType engine,
    IServiceProvider serviceProvider, 
    IGameEngineErrorReporter? errorReporter)
    : PetroglyphXmlFileParserBase(serviceProvider, errorReporter), IXmlContainerFileParser<GameObjectType>
{
    public event EventHandler<GameObjectParsedEventArgs>? GameObjectParsed;

    private readonly GameObjectTypeParser _gameObjectTypeParser = new(engine, serviceProvider, errorReporter);

    public INamedXmlObjectParser<GameObjectType> ElementParser => _gameObjectTypeParser;

    public bool OverlayLoad
    {
        get;
        set
        {
            field = value;
            _gameObjectTypeParser.OverlayLoad = value;
        }
    }

    public void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, GameObjectType> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out _);
        foreach (var xElement in root.Elements())
        {
            var parsedElement = _gameObjectTypeParser.Parse(xElement, parsedEntries, out var entryCrc);
            if (!OverlayLoad)
            {
                parsedEntries.Add(entryCrc, parsedElement);
                GameObjectParsed?.Invoke(this, new GameObjectParsedEventArgs(parsedElement, true));
            }
        }
    }
}