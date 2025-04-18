﻿using System;
using PG.Commons.Data;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class NamedXmlObject(string name, Crc32 nameCrc, XmlLocationInfo location) : XmlObject(location), IHasCrc32
{ 
    public Crc32 Crc32 { get; } = nameCrc; 

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}