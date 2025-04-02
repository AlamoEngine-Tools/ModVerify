using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Engine;

public interface IGameManager<T>
{
    ICollection<T> Entries { get; }

    ICollection<Crc32> EntryKeys { get; }

    ReadOnlyFrugalList<T> GetEntries(Crc32 key);
}