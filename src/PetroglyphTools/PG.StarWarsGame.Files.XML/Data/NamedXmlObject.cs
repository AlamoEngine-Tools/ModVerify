using System;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Data;

public abstract class NamedXmlObject(string name, Crc32 nameCrc, XmlLocationInfo location) : XmlObject(location)
{ 
    public Crc32 Crc32 { get; } = nameCrc; 

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}