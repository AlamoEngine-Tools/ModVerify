using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Database;

internal class XmlDatabase<T>(IReadOnlyValueListDictionary<Crc32, T> parsedObjects, IServiceProvider serviceProvider) : IXmlDatabase<T>
    where T : XmlObject
{
    private readonly IServiceProvider _serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    private readonly IReadOnlyValueListDictionary<Crc32, T> _parsedObjects = parsedObjects ?? throw new ArgumentNullException(nameof(parsedObjects));

    public ICollection<T> Entries => _parsedObjects.Values;

    public ICollection<Crc32> EntryKeys => _parsedObjects.Keys;

    public ReadOnlyFrugalList<T> GetEntries(Crc32 key)
    {
        return _parsedObjects.GetValues(key);
    }
}