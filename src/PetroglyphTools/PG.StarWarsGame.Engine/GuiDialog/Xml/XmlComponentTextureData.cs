using System;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class XmlComponentTextureData(string componentId, IReadOnlyFrugalValueListDictionary<string, string> textures, XmlLocationInfo location)
    : XmlObject(location)
{ 
    public string Component { get; } = componentId ?? throw new ArgumentNullException(componentId);

    public IReadOnlyFrugalValueListDictionary<string, string> Textures { get; } = textures ?? throw new ArgumentNullException(nameof(textures));
}