namespace PG.StarWarsGame.Files.XML.Data;

public abstract class XmlObject(XmlLocationInfo location)
{
    public XmlLocationInfo Location { get; } = location;

    public virtual void CoerceValues()
    {
    }
}