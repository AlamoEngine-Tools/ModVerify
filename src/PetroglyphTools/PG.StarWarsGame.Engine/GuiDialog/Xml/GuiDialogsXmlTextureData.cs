using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class GuiDialogsXmlTextureData(IEnumerable<XmlComponentTextureData> textures, XmlLocationInfo location) : XmlObject(location)
{
    public string? MegaTexture { get; init; }

    public string? CompressedMegaTexture { get; init; }

    public IReadOnlyList<XmlComponentTextureData> Textures { get; } = textures.ToList();
}