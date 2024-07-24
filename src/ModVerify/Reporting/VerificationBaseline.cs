using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting;

public sealed class VerificationBaseline : IReadOnlyCollection<VerificationError>
{
    public static readonly VerificationBaseline Empty = new([]);

    private readonly HashSet<VerificationError> _errors;

    /// <inheritdoc />
    public int Count => _errors.Count;

    internal VerificationBaseline(JsonVerificationBaseline baseline)
    {
        _errors = new HashSet<VerificationError>(baseline.Errors.Select(x => new VerificationError(x)));
    }

    public VerificationBaseline(IEnumerable<VerificationError> errors)
    {
        _errors = new(errors);
    }

    public void ToJson(Stream stream)
    {
        JsonSerializer.Serialize(stream, new JsonVerificationBaseline(this), ModVerifyJsonSettings.JsonSettings);
    }

    public Task ToJsonAsync(Stream stream)
    {
        return JsonSerializer.SerializeAsync(stream, new JsonVerificationBaseline(this), ModVerifyJsonSettings.JsonSettings);
    }

    public static VerificationBaseline FromJson(Stream stream)
    {
        var baselineJson = JsonSerializer.Deserialize<JsonVerificationBaseline>(stream, JsonSerializerOptions.Default);
        if (baselineJson is null)
            throw new InvalidOperationException("Unable to deserialize baseline");
        return new VerificationBaseline(baselineJson);
    }

    /// <inheritdoc />
    public IEnumerator<VerificationError> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}