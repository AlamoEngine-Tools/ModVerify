using AET.ModVerify.Reporting.Json;
using AnakinRaW.CommonUtilities.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace ModVerify.Test.Reporting;

public sealed class ValueListDictionaryJsonConverterStringIntTest : ValueListDictionaryJsonConverterTestBase<string, int>
{
    protected override string CreateKey(int seed) => "key" + seed;

    protected override int CreateValue(int seed) => seed;

    [Fact]
    public void Read_StringKeys_ReproducesDictionary()
    {
        const string json = """
                            {
                              "a": [1, 2, 3],
                              "b": [4]
                            }
                            """;

        var result = JsonSerializer.Deserialize<ReadOnlyValueListDictionary<string, int>>(json, Options);

        Assert.NotNull(result);
        Assert.Equal([1, 2, 3], result.GetValues("a"));
        Assert.Equal([4], result.GetValues("b"));
    }

    [Fact]
    public void Read_DroppingComparer_UsesDefaultComparerOnRoundTrip()
    {
        var mutable = new ValueListDictionary<string, int>(StringComparer.OrdinalIgnoreCase) { { "Key", 1 } };
        var original = new ReadOnlyValueListDictionary<string, int>(mutable);

        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<ReadOnlyValueListDictionary<string, int>>(json, Options);

        Assert.NotNull(result);
        // The original matched case-insensitively; the deserialized one uses the default comparer.
        Assert.Equal([1], result.GetValues("Key"));
        Assert.False(result.ContainsKey("key"));
    }
}

public sealed class ValueListDictionaryJsonConverterIntStringTest : ValueListDictionaryJsonConverterTestBase<int, string>
{
    protected override int CreateKey(int seed) => seed + 1;

    protected override string CreateValue(int seed) => "v" + seed;

    [Fact]
    public void Read_IntKeys_ParsesPropertyNames()
    {
        const string json = """{ "1": ["x"], "2": ["y", "z"] }""";

        var result = JsonSerializer.Deserialize<ReadOnlyValueListDictionary<int, string>>(json, Options);

        Assert.NotNull(result);
        Assert.Equal(["x"], result.GetValues(1));
        Assert.Equal(["y", "z"], result.GetValues(2));
    }
}

public abstract class ValueListDictionaryJsonConverterTestBase<TKey, TValue>
    where TKey : notnull
{
    protected static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add((JsonConverter)Activator.CreateInstance(typeof(ValueListDictionaryJsonConverter))!);
        return options;
    }

    protected abstract TKey CreateKey(int seed);

    protected abstract TValue CreateValue(int seed);

    // Canonical content shared by every case: key0 -> [v0, v1], key1 -> [v2].
    private IReadOnlyList<KeyValuePair<TKey, TValue[]>> ExpectedEntries()
    {
        return
        [
            new KeyValuePair<TKey, TValue[]>(CreateKey(0), [CreateValue(0), CreateValue(1)]),
            new KeyValuePair<TKey, TValue[]>(CreateKey(1), [CreateValue(2)]),
        ];
    }

    public static IEnumerable<object[]> Variants()
    {
        yield return [typeof(ValueListDictionary<TKey, TValue>)];
        yield return [typeof(FrugalValueListDictionary<TKey, TValue>)];
        yield return [typeof(ReadOnlyValueListDictionary<TKey, TValue>)];
        yield return [typeof(ReadOnlyFrugalValueListDictionary<TKey, TValue>)];
        yield return [typeof(IValueListDictionary<TKey, TValue>)];
        yield return [typeof(IReadOnlyValueListDictionary<TKey, TValue>)];
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Serialize_AnyInputType_ProducesSameJsonAsRuntimeDictionaryOfLists(Type declaredType)
    {
        var dictionary = CreateVariant(declaredType);

        var runtimeEquivalent = new Dictionary<TKey, List<TValue>>();
        foreach (var entry in ExpectedEntries())
            runtimeEquivalent[entry.Key] = [..entry.Value];

        var actual = JsonSerializer.Serialize(dictionary, declaredType, Options);
        var expected = JsonSerializer.Serialize(runtimeEquivalent, Options);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Deserialize_AnyOutputType_ReproducesContent(Type targetType)
    {
        var json = JsonSerializer.Serialize(
            CreateVariant(typeof(ValueListDictionary<TKey, TValue>)),
            typeof(ValueListDictionary<TKey, TValue>),
            Options);

        var result = (IReadOnlyValueListDictionary<TKey, TValue>?)
            JsonSerializer.Deserialize(json, targetType, Options);

        Assert.NotNull(result);
        foreach (var entry in ExpectedEntries())
            Assert.Equal(entry.Value, result.GetValues(entry.Key));
    }

    [Fact]
    public void RoundTrip_PreservesContent()
    {
        var original = CreateVariant(typeof(ReadOnlyValueListDictionary<TKey, TValue>));

        var json = JsonSerializer.Serialize(original, typeof(ReadOnlyValueListDictionary<TKey, TValue>), Options);
        var result = JsonSerializer.Deserialize<ReadOnlyValueListDictionary<TKey, TValue>>(json, Options);

        Assert.NotNull(result);
        foreach (var entry in ExpectedEntries())
            Assert.Equal(entry.Value, result.GetValues(entry.Key));
    }

    [Fact]
    public void Read_Null_ReturnsNull()
    {
        var result = JsonSerializer.Deserialize<ReadOnlyValueListDictionary<TKey, TValue>>("null", Options);
        Assert.Null(result);
    }

    [Fact]
    public void Read_ValueIsNotAnArray_Throws()
    {
        // A well-formed object whose value is a scalar instead of the expected array.
        var keyName = JsonSerializer.Serialize(CreateKey(0).ToString(), Options);
        var malformed = "{ " + keyName + ": 5 }";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ReadOnlyValueListDictionary<TKey, TValue>>(malformed, Options));
    }

    private object CreateVariant(Type declaredType)
    {
        var mutable = new ValueListDictionary<TKey, TValue>();
        var frugal = new FrugalValueListDictionary<TKey, TValue>();
        foreach (var entry in ExpectedEntries())
        {
            foreach (var value in entry.Value)
            {
                mutable.Add(entry.Key, value);
                frugal.Add(entry.Key, value);
            }
        }

        if (declaredType == typeof(FrugalValueListDictionary<TKey, TValue>))
            return frugal;
        if (declaredType == typeof(ReadOnlyValueListDictionary<TKey, TValue>)
            || declaredType == typeof(IReadOnlyValueListDictionary<TKey, TValue>))
            return new ReadOnlyValueListDictionary<TKey, TValue>(mutable);
        if (declaredType == typeof(ReadOnlyFrugalValueListDictionary<TKey, TValue>))
            return new ReadOnlyFrugalValueListDictionary<TKey, TValue>(frugal);

        // ValueListDictionary<,> and IValueListDictionary<,>.
        return mutable;
    }
}
