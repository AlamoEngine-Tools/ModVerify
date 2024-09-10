using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class GuiDialogsXmlTextureData(IEnumerable<XmlComponentTextureData> textures) // : XmlObject()
{
    public string? MegaTexture { get; init; }

    public string? CompressedMegaTexture { get; init; }

    public IReadOnlyList<XmlComponentTextureData> Textures { get; } = textures.ToList();
}