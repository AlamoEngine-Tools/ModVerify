using System;
using PG.Commons.DataTypes;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public abstract class XmlObject(
    string name,
    Crc32 nameCrc,
    XmlLocationInfo location)
    : IHasCrc32
{
    public XmlLocationInfo Location { get; } = location;

    public Crc32 Crc32 { get; } = nameCrc;

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}