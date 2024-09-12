using System;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class XmlComponentTextureData(string componentId, IReadOnlyValueListDictionary<string, string> textures, XmlLocationInfo location)
    : XmlObject(location)
{ 
    public string Component { get; } = componentId ?? throw new ArgumentNullException(componentId);

    public IReadOnlyValueListDictionary<string, string> Textures { get; } = textures ?? throw new ArgumentNullException(nameof(textures));
}