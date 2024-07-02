using System;
using PG.Commons.DataTypes;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public abstract class XmlObject(
    string name,
    Crc32 nameCrc,
    IReadOnlyValueListDictionary<string, object?> properties,
    XmlLocationInfo location)
    : IHasCrc32
{
    public XmlLocationInfo Location { get; } = location;

    public Crc32 Crc32 { get; } = nameCrc;

    public IReadOnlyValueListDictionary<string, object?> XmlProperties { get; } = properties ?? throw new ArgumentNullException(nameof(properties));

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public T? GetLastPropertyOrDefault<T>(string tagName, T? defaultValue = default)
    {
        if (!XmlProperties.TryGetLastValue(tagName, out var value))
            return defaultValue;
        return (T)value;
    }

    protected T LazyInitValue<T>(ref T? field, string tag, T defaultValue, Func<T, T>? coerceFunc = null)
    {
        if (field is null)
        {
            if (XmlProperties.TryGetLastValue(tag, out var value))
            {
                var tValue = (T)value;
                if (coerceFunc is not null)
                    tValue = coerceFunc(tValue);
                field = tValue;
            }
            else
                field = defaultValue;
        }

        return field;
    }
}