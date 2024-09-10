using System;
using PG.Commons.DataTypes;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public abstract class XmlObject(IReadOnlyValueListDictionary<string, object?> properties, XmlLocationInfo location)
{
    public XmlLocationInfo Location { get; } = location;

    public IReadOnlyValueListDictionary<string, object?> XmlProperties { get; } = properties ?? throw new ArgumentNullException(nameof(properties));

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

public abstract class NamedXmlObject(
    string name,
    Crc32 nameCrc,
    IReadOnlyValueListDictionary<string, object?> properties,
    XmlLocationInfo location)
    : XmlObject(properties, location), IHasCrc32
{ 
    public Crc32 Crc32 { get; } = nameCrc; 

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}