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
    private static readonly Version LatestVersion = new(2, 0);

    public static readonly VerificationBaseline Empty = new(VerificationSeverity.Information, []);

    private readonly HashSet<VerificationError> _errors;

    public Version? Version { get; }

    public VerificationSeverity MinimumSeverity { get; }

    /// <inheritdoc />
    public int Count => _errors.Count;

    internal VerificationBaseline(JsonVerificationBaseline baseline)
    {
        _errors = [..baseline.Errors.Select(x => new VerificationError(x))];
        Version = baseline.Version;
        MinimumSeverity = baseline.MinimumSeverity;
    }

    public VerificationBaseline(VerificationSeverity minimumSeverity, IEnumerable<VerificationError> errors)
    {
        _errors = [..errors];
        Version = LatestVersion;
        MinimumSeverity = minimumSeverity;
    }

    public bool Contains(VerificationError error)
    {
        return _errors.Contains(error);
    }

    public IEnumerable<VerificationError> Apply(IEnumerable<VerificationError> errors)
    {
        return Count == 0 ? errors : errors.Where(e => !_errors.Contains(e));
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
            throw new InvalidOperationException("Unable to deserialize baseline.");

        if (baselineJson.Version is null || baselineJson.Version != LatestVersion)
            throw new IncompatibleBaselineException();

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

    public override string ToString()
    {
        return $"Baseline [Version={Version}, MinSeverity={MinimumSeverity}, NumErrors={Count}]";
    }
}