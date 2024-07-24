using AET.ModVerify.Reporting.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AET.ModVerify.Reporting;

public sealed class SuppressionList : IReadOnlyCollection<SuppressionFilter>
{
    public static readonly SuppressionList Empty = new([]);

    private readonly IReadOnlyCollection<SuppressionFilter> _filters;
    private readonly IReadOnlyCollection<SuppressionFilter> _minimizedFilters;

    public int Count => _filters.Count;

    public SuppressionList(IEnumerable<SuppressionFilter> suppressionFilters)
    {
        if (suppressionFilters == null) 
            throw new ArgumentNullException(nameof(suppressionFilters));

        _filters = new List<SuppressionFilter>(suppressionFilters);
        _minimizedFilters = MinimizeSuppressions(_filters);
    }

    internal SuppressionList(JsonSuppressionList suppressionList)
    {
        if (suppressionList == null)
            throw new ArgumentNullException(nameof(suppressionList));

        _filters = suppressionList.Filters.Select(x => new SuppressionFilter(x)).ToList();
        _minimizedFilters = MinimizeSuppressions(_filters);
    }

    public void ToJson(Stream stream)
    {
        JsonSerializer.Serialize(stream, new JsonSuppressionList(this), ModVerifyJsonSettings.JsonSettings);
    }

    public static SuppressionList FromJson(Stream stream)
    {
        var baselineJson = JsonSerializer.Deserialize<JsonSuppressionList>(stream, JsonSerializerOptions.Default);
        if (baselineJson is null)
            throw new InvalidOperationException("Unable to deserialize baseline");
        return new SuppressionList(baselineJson);
    }


    public bool Suppresses(VerificationError error)
    {
        foreach (var filter in _minimizedFilters)
        {
            if (filter.Suppresses(error))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyCollection<SuppressionFilter> MinimizeSuppressions(IEnumerable<SuppressionFilter> filters)
    {
        var sortedFilters = filters.Where(f => !f.IsDisabled())
            .OrderBy(x => x.Specificity());

        var result = new List<SuppressionFilter>();

        foreach (var filter in sortedFilters)
        {
            if (result.All(x => !filter.IsSupersededBy(x)))
                result.Add(filter);
        }

        return result;
    }

    public IEnumerator<SuppressionFilter> GetEnumerator()
    {
        return _filters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}