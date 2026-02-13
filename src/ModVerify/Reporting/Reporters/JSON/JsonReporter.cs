using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;
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

    private static JsonVerificationReport CreateJsonReport(VerificationResult result)
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
            Errors = result.Errors.Select(x => new JsonVerificationError(x))
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
}