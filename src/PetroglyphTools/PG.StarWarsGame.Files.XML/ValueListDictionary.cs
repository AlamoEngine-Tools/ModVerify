using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AnakinRaW.CommonUtilities.Collections;

namespace PG.StarWarsGame.Files.XML;


public interface IReadOnlyValueListDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    ICollection<TValue> Values { get; }
    ICollection<TKey> Keys { get; }

    bool ContainsKey(TKey key);

    ReadOnlyFrugalList<TValue> GetValues(TKey key);

    TValue GetLastValue(TKey key);

    TValue GetFirstValue(TKey key);

    bool TryGetFirstValue(TKey key, [NotNullWhen(true)] out TValue value);

    bool TryGetLastValue(TKey key, [NotNullWhen(true)] out TValue value);

    bool TryGetValues(TKey key, out ReadOnlyFrugalList<TValue> values);
}

public interface IValueListDictionary<TKey, TValue> : IReadOnlyValueListDictionary<TKey, TValue> where TKey : notnull
{
    bool Add(TKey key, TValue value);
}

// NOT THREAD-SAFE!
public class ValueListDictionary<TKey, TValue> : IValueListDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _singleValueDictionary = new ();
    private readonly Dictionary<TKey, List<TValue>> _multiValueDictionary = new();

    public ICollection<TKey> Keys => _singleValueDictionary.Keys.Concat(_multiValueDictionary.Keys).ToList();

    public ICollection<TValue> Values => this.Select(x => x.Value).ToList();

    public bool ContainsKey(TKey key)
    {
        return _singleValueDictionary.ContainsKey(key) || _multiValueDictionary.ContainsKey(key);
    }
    
    public bool Add(TKey key, TValue value)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (!_singleValueDictionary.ContainsKey(key))
        {
            if (!_multiValueDictionary.TryGetValue(key, out var list))
            {
                _singleValueDictionary.Add(key, value);
                return false;
            }

            list.Add(value);
            return true;
        }

        Debug.Assert(_multiValueDictionary.ContainsKey(key) == false);

        var firstValue = _singleValueDictionary[key];
        _singleValueDictionary.Remove(key);

        _multiValueDictionary.Add(key, [
            firstValue,
            value
        ]);

        return true;
    }

    public TValue GetLastValue(TKey key)
    {
        if (_singleValueDictionary.TryGetValue(key, out var value))
            return value;

        if (_multiValueDictionary.TryGetValue(key, out var valueList))
            return valueList.Last();

        throw new KeyNotFoundException($"The key '{key}' was not found.");
    }

    public TValue GetFirstValue(TKey key)
    {
        if (_singleValueDictionary.TryGetValue(key, out var value))
            return value;

        if (_multiValueDictionary.TryGetValue(key, out var valueList))
            return valueList.First();

        throw new KeyNotFoundException($"The key '{key}' was not found.");
    }
    
    public ReadOnlyFrugalList<TValue> GetValues(TKey key)
    {
        if (TryGetValues(key, out var values)) 
            return values;

        throw new KeyNotFoundException($"The key '{key}' was not found.");

    }

    public bool TryGetFirstValue(TKey key, [NotNullWhen(true)] out TValue value)
    {
        if (_singleValueDictionary.TryGetValue(key, out value!))
            return true;

        if (_multiValueDictionary.TryGetValue(key, out var valueList))
        {
            value = valueList.First()!;
            return true;
        }

        return false;
    }

    public bool TryGetLastValue(TKey key, [NotNullWhen(true)] out TValue value)
    {
        if (_singleValueDictionary.TryGetValue(key, out value!))
            return true;

        if (_multiValueDictionary.TryGetValue(key, out var valueList))
        {
            value = valueList.Last()!;
            return true;
        }

        return false;
    }

    public bool TryGetValues(TKey key, out ReadOnlyFrugalList<TValue> values)
    {
        if (_singleValueDictionary.TryGetValue(key, out var value))
        {
            values = new ReadOnlyFrugalList<TValue>(value);
            return true;
        }

        if (_multiValueDictionary.TryGetValue(key, out var valueList))
        {
            values = new ReadOnlyFrugalList<TValue>(valueList);
            return true;
        }

        values = ReadOnlyFrugalList<TValue>.Empty;
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue>.Enumerator _singleEnumerator;
        private Dictionary<TKey, List<TValue>>.Enumerator _multiEnumerator;
        private List<TValue>.Enumerator _currentListEnumerator = default;
        private bool _isMultiEnumeratorActive = false;

        internal Enumerator(ValueListDictionary<TKey, TValue> valueListDictionary)
        {
            _singleEnumerator = valueListDictionary._singleValueDictionary.GetEnumerator();
            _multiEnumerator = valueListDictionary._multiValueDictionary.GetEnumerator();
        }

        public KeyValuePair<TKey, TValue> Current =>
            _isMultiEnumeratorActive
                ? new KeyValuePair<TKey, TValue>(_multiEnumerator.Current.Key, _currentListEnumerator.Current)
                : _singleEnumerator.Current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_singleEnumerator.MoveNext())
                return true;

            if (_isMultiEnumeratorActive)
            {
                if (_currentListEnumerator.MoveNext())
                    return true;
                _isMultiEnumeratorActive = false;
            }

            if (_multiEnumerator.MoveNext())
            {
                _currentListEnumerator = _multiEnumerator.Current.Value.GetEnumerator();
                _isMultiEnumeratorActive = true;
                return _currentListEnumerator.MoveNext();
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _singleEnumerator.Dispose();
            _multiEnumerator.Dispose();
        }
    }
}

public static class ValueListDictionaryExtensions
{
    public static IEnumerable<T> AggregateValues<TKey, TValue, T>(
        this IReadOnlyValueListDictionary<TKey, TValue> valueListDictionary,
        ISet<TKey> keys, Predicate<T> filter,
        AggregateStrategy aggregateStrategy)
        where TKey : notnull 
        where T : TValue
    {
        foreach (var key in keys)
        {
            if (!valueListDictionary.ContainsKey(key))
                continue;
            if (aggregateStrategy == AggregateStrategy.MultipleValuesPerKey)
            {
                foreach (var value in valueListDictionary.GetValues(key))
                {
                    if (value is not null)
                    {
                        var typedValue = (T)value;
                        if (filter(typedValue))
                            yield return typedValue;
                    }

                }
            }
            else
            {
                var value = aggregateStrategy == AggregateStrategy.FirstValuePerKey
                    ? valueListDictionary.GetFirstValue(key)
                    : valueListDictionary.GetLastValue(key);
                if (value is not null)
                {
                    var typedValue = (T)value;
                    if (filter(typedValue))
                        yield return typedValue;
                }
            }
        }
    }

    public enum AggregateStrategy
    {
        FirstValuePerKey,
        LastValuePerKey,
        MultipleValuesPerKey,
    }
}