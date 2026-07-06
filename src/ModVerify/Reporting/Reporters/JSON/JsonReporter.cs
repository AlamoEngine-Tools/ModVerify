using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.Collections;
using AnakinRaW.CommonUtilities.FileSystem.Validation;

namespace AET.ModVerify.Reporting.Reporters;

internal class JsonReporter(JsonReporterSettings settings, IServiceProvider serviceProvider) 
    : FileBasedReporter<JsonReporterSettings>(settings, serviceProvider)
{
    public override async Task ReportAsync(VerificationResult verificationResult)
    {
        var report = CreateJsonReport(verificationResult);
        var fileName = CreateFileName(verificationResult);

#if NET || NETSTANDARD2_1
        await 
#endif
        using var fs = CreateFile(fileName);
        await JsonSerializer.SerializeAsync(fs, report, ModVerifyJsonSettings.JsonSettings);
    }

    private JsonVerificationReport CreateJsonReport(VerificationResult result)
    {
        return new JsonVerificationReport
        {
            Metadata = new JsonVerificationReportMetadata
            {
                Target = new JsonVerificationTarget(result.Target),
                Duration = result.Duration.ToString("g"),
                Status = result.Status,
                Verifiers = result.Verifiers.Select(x => x.Name).ToList()
            },
            Errors = ToJsonErrors(result.Errors.NewErrors),
            Resolved = ToJsonResolved(result.Errors.ResolvedErrors)
        };
    }

    private IReadOnlyList<JsonVerificationErrorBase> ToJsonErrors(IEnumerable<VerificationError> errors)
    {
        var ordered = errors
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Id);

        if (!Settings.AggregateResults)
        {
            return ordered
                .Select(x => (JsonVerificationErrorBase)new JsonVerificationError(x, Settings.Verbose))
                .ToList();
        }

        return ordered
            .GroupBy(x => new GroupKey(x.Asset, x.Id, x.VerifierChain))
            .Select<IGrouping<GroupKey, VerificationError>, JsonVerificationErrorBase>(g =>
            {
                var first = g.First();
                var contexts = g.Select(x => x.ContextEntries).ToList();
                if (contexts.Count == 1)
                    return new JsonVerificationError(first, Settings.Verbose);
                return new JsonAggregatedVerificationError(first, contexts, Settings.Verbose);
            })
            .ToList();
    }

    private ReadOnlyValueListDictionary<string, JsonVerificationErrorBase>? ToJsonResolved(
        IReadOnlyValueListDictionary<string, VerificationError> resolvedErrors)
    {
        var resolved = new ValueListDictionary<string, JsonVerificationErrorBase>();
        foreach (var baseline in resolvedErrors)
        {
            if (baseline.Value.Count == 0)
                continue;
            resolved.AddRange(baseline.Key, ToJsonErrors(baseline.Value));
        }

        return resolved.Count == 0 ? null : new ReadOnlyValueListDictionary<string, JsonVerificationErrorBase>(resolved);
    }

    private static string CreateFileName(VerificationResult result)
    {
        var fileName = $"VerificationResult_{result.Target.Name}.json";
        if (CurrentSystemFileNameValidator.Instance.IsValidFileName(fileName) is FileNameValidationResult.Valid)
            return fileName;
        // I don't think there is a safe/secure way to re-encode the file name, if it's not valid using the plain target name.
        // Thus, we simply use the current date and accept the fact that files may get overwritten for different targets.
        return $"VerificationResult_{DateTime.Now:yyyy_mm_dd}.json";

    }

    private readonly record struct GroupKey(string Asset, string Id, IReadOnlyList<IGameVerifierInfo> VerifierChain)
    {
        public bool Equals(GroupKey other)
        {
            return Asset == other.Asset
                   && Id == other.Id
                   && VerifierChainEqualityComparer.Instance.Equals(VerifierChain, other.VerifierChain);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Asset);
            hashCode.Add(Id);
            hashCode.Add(VerifierChain, VerifierChainEqualityComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}