using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.IO;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class GameObjectParsedEventArgs : EventArgs
{
    public bool Unique { get; }

    public GameObject ParsedElement { get; }

    internal GameObjectParsedEventArgs(GameObject parsedElement, bool unique)
    {
        Unique = unique;
        ParsedElement = parsedElement ?? throw new ArgumentNullException(nameof(parsedElement));
    }
}


internal class GameObjectFileParser(IServiceProvider serviceProvider, IGameEngineErrorReporter? errorReporter)
    : PetroglyphXmlFileParserBase(serviceProvider, errorReporter), IXmlContainerFileParser<GameObject>
{
    public event EventHandler<GameObjectParsedEventArgs>? GameObjectParsed;

    private readonly GameObjectParser _gameObjectParser = new(serviceProvider, errorReporter);

    public INamedXmlObjectParser<GameObject> ElementParser => _gameObjectParser;

    public bool OverlayLoad
    {
        get;
        set
        {
            field = value;
            _gameObjectParser.OverlayLoad = value;
        }
    }

    public void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, GameObject> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out _);
        foreach (var xElement in root.Elements())
        {
            var parsedElement = _gameObjectParser.Parse(xElement, parsedEntries, out var entryCrc);
            if (!OverlayLoad)
            {
                parsedEntries.Add(entryCrc, parsedElement);
                GameObjectParsed?.Invoke(this, new GameObjectParsedEventArgs(parsedElement, true));
            }
        }
    }
}