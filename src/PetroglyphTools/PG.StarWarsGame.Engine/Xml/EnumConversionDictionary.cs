using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Engine.Xml;

public sealed class EnumConversionDictionary<T> : IReadOnlyCollection<KeyValuePair<string, T>> where T : struct, Enum
{
    // This is the value the engine would give you.
    internal const string StringNotFoundDummy = "-BAD VALUE-";

    // Most consumers call the method TryStringToEnum. Thus, we optimize the class for this case
    // and accept performance penalties for the EnumToString case.
    private readonly IReadOnlyDictionary<string, T> _dictionary;

    public int Count => _dictionary.Count;

    public EnumConversionDictionary(IEnumerable<KeyValuePair<string, T>> entries)
    {
        var dictionary = new Dictionary<string, T>();
        var values = dictionary.Values;
        foreach (var entry in entries)
        {
            if (values.Contains(entry.Value))
                throw new InvalidOperationException($"Enum value {entry.Value} already exists!");
            dictionary.Add(entry.Key.ToUpperInvariant(), entry.Value);
        }
        _dictionary = dictionary;
    }

    public bool TryStringToEnum(string key, out T enumValue)
    {
        key = key.ToUpperInvariant();
        return _dictionary.TryGetValue(key, out enumValue);
    }

    public string EnumToString(T enumValue)
    {
        foreach (var keyValuePair in _dictionary)
        {
            if (EqualityComparer<T>.Default.Equals(enumValue, keyValuePair.Value))
                return keyValuePair.Key;
        }

        return StringNotFoundDummy;
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}