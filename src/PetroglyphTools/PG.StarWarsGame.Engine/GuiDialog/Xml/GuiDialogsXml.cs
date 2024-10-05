using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class GuiDialogsXml(GuiDialogsXmlTextureData textureData, XmlLocationInfo location) : XmlObject(location)
{
    public GuiDialogsXmlTextureData TextureData { get; } = textureData;
}