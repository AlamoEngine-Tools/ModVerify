using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public sealed class SfxEvent : XmlObject
{
    private int _volumeValue;

    public bool IsPreset { get; }

    public SfxEvent? Preset { get; }

    public int Volume => Preset?.Volume ?? _volumeValue;

    internal SfxEvent(string name, Crc32 nameCrc, XmlLocationInfo location) 
        : base(name, nameCrc, location)
    {
    }
}