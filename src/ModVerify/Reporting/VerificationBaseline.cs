using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting;

public sealed class VerificationBaseline : IReadOnlyCollection<VerificationError>
{
    public static readonly Version LatestVersion = new(2, 1);
    public static readonly string LatestVersionString = LatestVersion.ToString(2);

    public static readonly VerificationBaseline Empty = new(VerificationSeverity.Information, [], null);

    private readonly HashSet<VerificationError> _errors;

    public VerificationTarget? Target { get; }
    
    public Version? Version { get; }

    public VerificationSeverity MinimumSeverity { get; }

    /// <inheritdoc />
    public int Count => _errors.Count;

    internal VerificationBaseline(JsonVerificationBaseline baseline)
    {
        _errors = [..baseline.Errors.Select(x => new VerificationError(x))];
        Version = baseline.Version;
        MinimumSeverity = baseline.MinimumSeverity;
        Target = JsonVerificationTarget.ToTarget(baseline.Target);
    }

    public VerificationBaseline(VerificationSeverity minimumSeverity, IEnumerable<VerificationError> errors, VerificationTarget? target)
    {
        _errors = [..errors];
        Version = LatestVersion;
        MinimumSeverity = minimumSeverity;
        Target = target;
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
        return JsonBaselineParser.Parse(stream);
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
        var sb = new StringBuilder($"Baseline [Version={Version}, MinSeverity={MinimumSeverity}, NumErrors={Count}");
        if (Target is not null)
            sb.Append($", Target={Target}");
        sb.Append(']');
        return sb.ToString();
    }
}

public sealed class BaselineVerificationTarget
{
    public required GameEngineType Engine { get; init; }
    public required string Name { get; init; }
    public GameLocations? Location { get; init; }
    public string? Version { get; init; }
    public bool IsGame => Location.ModPaths.Count == 0;
    
    

    public override string ToString()
    {
        var sb = new StringBuilder($"[Name={Name};EngineType={Engine};");
        if (!string.IsNullOrEmpty(Version)) sb.Append($"Version={Version};");
        sb.Append($"Location={Location};");
        sb.Append(']');
        return sb.ToString();
    }
}