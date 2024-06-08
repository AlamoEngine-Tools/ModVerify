using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;

namespace PG.StarWarsGame.Engine.Database;

public interface IXmlDatabase<T> where T : XmlObject
{
    ICollection<T> Entries { get; }

    ICollection<Crc32> EntryKeys { get; }

    ReadOnlyFrugalList<T> GetEntries(Crc32 key);
}