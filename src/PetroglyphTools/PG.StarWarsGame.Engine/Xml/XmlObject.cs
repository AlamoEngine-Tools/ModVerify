using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class XmlObject(XmlLocationInfo location)
{
    public XmlLocationInfo Location { get; } = location;

    internal virtual void CoerceValues()
    {
    }
}