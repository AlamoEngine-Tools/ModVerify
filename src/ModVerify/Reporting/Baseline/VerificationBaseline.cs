using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Baseline.Json;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting.Baseline;

/// <summary>Represents a frozen set of verification errors that are already known and should be ignored during verification.</summary>
public sealed class VerificationBaseline : IReadOnlyCollection<VerificationError>
{
    /// <summary>Gets the latest supported baseline format version.</summary>
    public static readonly Version LatestVersion = new(2, 2);

    /// <summary>Gets the latest supported baseline format version as a string with two components.</summary>
    public static readonly string LatestVersionString = LatestVersion.ToString(2);

    /// <summary>Gets an empty <see cref="VerificationBaseline"/> that contains no errors.</summary>
    public static readonly VerificationBaseline Empty = new(VerificationSeverity.Information, [], null);

    private readonly HashSet<VerificationError> _errors;

    /// <summary>Gets the target that this baseline was created for, or <see langword="null"/> if not specified.</summary>
    public BaselineVerificationTarget? Target { get; }

    /// <summary>Gets the format version of this baseline, or <see langword="null"/> if unknown.</summary>
    public Version? Version { get; }

    /// <summary>Gets the minimum severity of the errors recorded in this baseline.</summary>
    public VerificationSeverity MinimumSeverity { get; }

    /// <inheritdoc />
    public int Count => _errors.Count;

    /// <summary>Gets a value that indicates whether the baseline contains no errors.</summary>
    /// <value><see langword="true"/> if the baseline contains no errors; otherwise, <see langword="false"/>.</value>
    public bool IsEmpty => Count == 0;

    internal VerificationBaseline(JsonVerificationBaseline baseline)
    {
        _errors = [..baseline.Errors.Select(x => new VerificationError(x))];
        Version = baseline.Version;
        MinimumSeverity = baseline.MinimumSeverity;
        Target = JsonVerificationTarget.ToTarget(baseline.Target);
    }

    /// <summary>Initializes a new instance of the <see cref="VerificationBaseline"/> class.</summary>
    /// <param name="minimumSeverity">One of the enumeration values that specifies the minimum severity of the errors recorded in the baseline.</param>
    /// <param name="errors">The errors to record in the baseline.</param>
    /// <param name="target">The target the baseline was created for, or <see langword="null"/> if not specified.</param>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    public VerificationBaseline(VerificationSeverity minimumSeverity, IEnumerable<VerificationError> errors, BaselineVerificationTarget? target)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        _errors = [..errors];
        Version = LatestVersion;
        MinimumSeverity = minimumSeverity;
        Target = target;
    }

    /// <summary>Determines whether the baseline contains the specified error.</summary>
    /// <param name="error">The verification error to locate.</param>
    /// <returns><see langword="true"/> if the baseline contains <paramref name="error"/>; otherwise, <see langword="false"/>.</returns>
    public bool Contains(VerificationError error)
    {
        return _errors.Contains(error);
    }

    /// <summary>Filters out the errors that are contained in the baseline.</summary>
    /// <param name="errors">The errors to filter.</param>
    /// <returns>The errors that are not contained in the baseline.</returns>
    public IEnumerable<VerificationError> Apply(IEnumerable<VerificationError> errors)
    {
        return Count == 0 ? errors : errors.Where(e => !_errors.Contains(e));
    }

    /// <summary>Serializes the baseline as JSON to the specified stream.</summary>
    /// <param name="stream">The stream to write the JSON representation to.</param>
    public void ToJson(Stream stream)
    {
        JsonSerializer.Serialize(stream, new JsonVerificationBaseline(this), ModVerifyJsonSettings.JsonSettings);
    }

    /// <summary>Asynchronously serializes the baseline as JSON to the specified stream.</summary>
    /// <param name="stream">The stream to write the JSON representation to.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    public Task ToJsonAsync(Stream stream)
    {
        return JsonSerializer.SerializeAsync(stream, new JsonVerificationBaseline(this), ModVerifyJsonSettings.JsonSettings);
    }

    /// <summary>Deserializes a baseline from its JSON representation.</summary>
    /// <param name="stream">The stream to read the JSON representation from.</param>
    /// <returns>The deserialized baseline.</returns>
    /// <exception cref="InvalidBaselineException">The JSON representation is invalid or cannot be parsed.</exception>
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

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder($"Baseline [Version={Version}, MinSeverity={MinimumSeverity}, NumErrors={Count}");
        if (Target is not null)
            sb.Append($", Target={Target}");
        sb.Append(']');
        return sb.ToString();
    }
}