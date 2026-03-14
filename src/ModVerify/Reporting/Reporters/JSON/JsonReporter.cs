using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;
using AET.ModVerify.Verifiers;
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
        IEnumerable<JsonVerificationErrorBase> errors;
        if (Settings.AggregateResults)
        {
            errors = result.Errors
                .OrderBy(x => x.Id)
                .GroupBy(x => new GroupKey(x.Asset, x.Id, x.VerifierChain))
                .Select<IGrouping<GroupKey, VerificationError>, JsonVerificationErrorBase>(g =>
                {
                    var first = g.First();
                    var contexts = g.Select(x => x.ContextEntries).ToList();

                    if (contexts.Count == 1)
                        return new JsonVerificationError(first);
                    return new JsonAggregatedVerificationError(first, contexts);
                });
        }
        else
        {
            errors = result.Errors
                .OrderBy(x => x.Id)
                .Select(x => new JsonVerificationError(x));
        }

        return new JsonVerificationReport
        {
            Metadata = new JsonVerificationReportMetadata
            {
                Target = new JsonVerificationTarget(result.Target),
                Duration = result.Duration.ToString("g"),
                Status = result.Status,
                Verifiers = result.Verifiers.Select(x => x.Name).ToList()
            },
            Errors = errors
        };
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