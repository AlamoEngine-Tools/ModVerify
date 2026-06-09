using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting.Json;

/// <summary>
/// Serializes any value-list-dictionary as a JSON object where each key maps to a JSON array of its
/// associated values, and reads the same shape back.
/// </summary>
/// <remarks>
/// <para>
/// Supports the mutable (<see cref="ValueListDictionary{TKey,TValue}"/>,
/// <see cref="FrugalValueListDictionary{TKey,TValue}"/>), read-only
/// (<see cref="ReadOnlyValueListDictionary{TKey,TValue}"/>,
/// <see cref="ReadOnlyFrugalValueListDictionary{TKey,TValue}"/>) and frugal variants, as well as their
/// corresponding interfaces.
/// </para>
/// <para>
/// String keys honor <see cref="JsonSerializerOptions.DictionaryKeyPolicy"/> on write, matching the
/// behavior of the runtime's built-in dictionary converter. Non-string keys are converted using the
/// invariant culture. The key equality comparer of a dictionary is part of its runtime state and
/// cannot be represented in JSON; deserialized dictionaries therefore always use the default comparer.
/// </para>
/// </remarks>
internal sealed class ValueListDictionaryJsonConverter : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return GetValueListDictionaryInterface(typeToConvert) is not null;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionaryInterface =
            GetValueListDictionaryInterface(typeToConvert) ??
            throw new ArgumentException($"Type '{typeToConvert}' is not a value-list-dictionary.", nameof(typeToConvert));

        var arguments = dictionaryInterface.GetGenericArguments();
        var converterType = typeof(Converter<,,>).MakeGenericType(typeToConvert, arguments[0], arguments[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static Type? GetValueListDictionaryInterface(Type typeToConvert)
    {
        if (typeToConvert is { IsInterface: true, IsGenericType: true } && 
            typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyValueListDictionary<,>))
        {
            return typeToConvert;
        }

        return typeToConvert.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyValueListDictionary<,>));
    }

    private sealed class Converter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
        where TKey : notnull
    {
        private readonly DictionaryKind _kind = DetermineKind(typeof(TDictionary));

        private static DictionaryKind DetermineKind(Type type)
        {
            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (definition == typeof(ReadOnlyFrugalValueListDictionary<,>) || 
                definition == typeof(IReadOnlyFrugalValueListDictionary<,>))
            {
                return DictionaryKind.ReadOnlyFrugal;
            }

            if (definition == typeof(FrugalValueListDictionary<,>) || 
                definition == typeof(IFrugalValueListDictionary<,>))
            {
                return DictionaryKind.Frugal;
            }

            if (definition == typeof(ReadOnlyValueListDictionary<,>) || 
                definition == typeof(IReadOnlyValueListDictionary<,>))
            {
                return DictionaryKind.ReadOnly;
            }

            // ValueListDictionary<,> and IValueListDictionary<,>.
            return DictionaryKind.Mutable;
        }

        public override TDictionary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected start of object but got '{reader.TokenType}'.");

            var entries = new List<KeyValuePair<TKey, List<TValue>>>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return (TDictionary)Build(entries);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"Expected property name but got '{reader.TokenType}'.");

                var key = ParseKey(reader.GetString() 
                                   ?? throw new JsonException("Property name must not be null."));

                reader.Read();
                if (reader.TokenType is not (JsonTokenType.StartArray or JsonTokenType.Null))
                    throw new JsonException($"Expected start of array for key '{key}' but got '{reader.TokenType}'.");
                
                var values = JsonSerializer.Deserialize<List<TValue>>(ref reader, options) ?? [];
                entries.Add(new KeyValuePair<TKey, List<TValue>>(key, values));
            }

            throw new JsonException("Unexpected end of JSON while reading object.");
        }

        private object Build(IReadOnlyList<KeyValuePair<TKey, List<TValue>>> entries)
        {
            switch (_kind)
            {
                case DictionaryKind.Frugal:
                    return BuildFrugal(entries);
                case DictionaryKind.ReadOnlyFrugal:
                    return new ReadOnlyFrugalValueListDictionary<TKey, TValue>(BuildFrugal(entries));
                case DictionaryKind.ReadOnly:
                    return new ReadOnlyValueListDictionary<TKey, TValue>(BuildMutable(entries));
                default:
                    return BuildMutable(entries);
            }
        }

        private static ValueListDictionary<TKey, TValue> BuildMutable(IReadOnlyList<KeyValuePair<TKey, List<TValue>>> entries)
        {
            var dictionary = new ValueListDictionary<TKey, TValue>();
            foreach (var entry in entries)
            {
                foreach (var value in entry.Value)
                    dictionary.Add(entry.Key, value);
            }

            return dictionary;
        }

        private static FrugalValueListDictionary<TKey, TValue> BuildFrugal(IReadOnlyList<KeyValuePair<TKey, List<TValue>>> entries)
        {
            var dictionary = new FrugalValueListDictionary<TKey, TValue>();
            foreach (var entry in entries)
            {
                foreach (var value in entry.Value)
                    dictionary.Add(entry.Key, value);
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
        {
            var dictionary = (IReadOnlyValueListDictionary<TKey, TValue>)value!;
            writer.WriteStartObject();
            foreach (var key in dictionary.Keys)
            {
                writer.WritePropertyName(ConvertKey(key, options));
                JsonSerializer.Serialize(writer, dictionary.GetValues(key), options);
            }
            writer.WriteEndObject();
        }

        private static string ConvertKey(TKey key, JsonSerializerOptions options)
        {
            if (key is string s)
                return options.DictionaryKeyPolicy?.ConvertName(s) ?? s;
            return Convert.ToString(key, CultureInfo.InvariantCulture)
                   ?? throw new NotSupportedException($"Unable to convert key of type '{typeof(TKey)}' to a JSON property name.");
        }

        private static TKey ParseKey(string propertyName)
        {
            if (typeof(TKey) == typeof(string))
                return (TKey)(object)propertyName;
            try
            {
                return (TKey)Convert.ChangeType(propertyName, typeof(TKey), CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException)
            {
                throw new JsonException($"Unable to convert JSON property name '{propertyName}' to a key of type '{typeof(TKey)}'.", e);
            }
        }
    }

    private enum DictionaryKind
    {
        Mutable,
        Frugal,
        ReadOnly,
        ReadOnlyFrugal
    }
}
