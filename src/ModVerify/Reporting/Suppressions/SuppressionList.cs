using AET.ModVerify.Reporting.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AET.ModVerify.Reporting.Suppressions.Json;

namespace AET.ModVerify.Reporting.Suppressions;

/// <summary>Represents a set of suppression filters that remove known verification errors from a result.</summary>
public sealed class SuppressionList : IReadOnlyCollection<SuppressionFilter>
{
    /// <summary>Gets an empty <see cref="SuppressionList"/> that suppresses nothing.</summary>
    public static readonly SuppressionList Empty = new([]);

    private readonly IReadOnlyCollection<SuppressionFilter> _filters;
    private readonly IReadOnlyCollection<SuppressionFilter> _minimizedFilters;

    /// <inheritdoc />
    public int Count => _filters.Count;

    /// <summary>Initializes a new instance of the <see cref="SuppressionList"/> class with the specified filters.</summary>
    /// <param name="suppressionFilters">The suppression filters to include in the list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="suppressionFilters"/> is <see langword="null"/>.</exception>
    public SuppressionList(IEnumerable<SuppressionFilter> suppressionFilters)
    {
        if (suppressionFilters == null) 
            throw new ArgumentNullException(nameof(suppressionFilters));

        _filters = [..suppressionFilters];
        _minimizedFilters = MinimizeSuppressions(_filters);
    }

    internal SuppressionList(JsonSuppressionList suppressionList)
    {
        if (suppressionList == null)
            throw new ArgumentNullException(nameof(suppressionList));

        _filters = suppressionList.Filters.Select(x => new SuppressionFilter(x)).ToList();
        _minimizedFilters = MinimizeSuppressions(_filters);
    }

    /// <summary>Serializes the suppression list as JSON to the specified stream.</summary>
    /// <param name="stream">The stream to write the JSON representation to.</param>
    public void ToJson(Stream stream)
    {
        JsonSerializer.Serialize(stream, new JsonSuppressionList(this), ModVerifyJsonSettings.JsonSettings);
    }

    /// <summary>Deserializes a suppression list from its JSON representation.</summary>
    /// <param name="stream">The stream to read the JSON representation from.</param>
    /// <returns>The deserialized suppression list.</returns>
    /// <exception cref="InvalidOperationException">The JSON representation cannot be deserialized.</exception>
    public static SuppressionList FromJson(Stream stream)
    {
        var baselineJson = JsonSerializer.Deserialize<JsonSuppressionList>(stream, JsonSerializerOptions.Default);
        if (baselineJson is null)
            throw new InvalidOperationException("Unable to deserialize baseline");
        return new SuppressionList(baselineJson);
    }

    /// <summary>Filters out the errors that are suppressed by any filter in the list.</summary>
    /// <param name="errors">The errors to filter.</param>
    /// <returns>The errors that are not suppressed by any filter in the list.</returns>
    public IEnumerable<VerificationError> Apply(IEnumerable<VerificationError> errors)
    {
        return Count == 0 ? errors : errors.Where(e => !Suppresses(e));
    }

    /// <summary>Determines whether any filter in the list suppresses the specified error.</summary>
    /// <param name="error">The verification error to test.</param>
    /// <returns><see langword="true"/> if a filter in the list suppresses <paramref name="error"/>; otherwise, <see langword="false"/>.</returns>
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
        var sortedFilters = filters.Where(f => !f.IsDisabled)
            .OrderBy(x => x.Specificity());

        var result = new List<SuppressionFilter>();

        foreach (var filter in sortedFilters)
        {
            if (result.All(x => !filter.IsSupersededBy(x)))
                result.Add(filter);
        }

        return result;
    }

    /// <inheritdoc />
    public IEnumerator<SuppressionFilter> GetEnumerator()
    {
        return _filters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}