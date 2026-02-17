using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Engine.GuiDialog.Xml;

public class GuiDialogsXml(GuiDialogsXmlTextureData textureData, XmlLocationInfo location) : XmlObject(location)
{
    public GuiDialogsXmlTextureData TextureData { get; } = textureData;
}