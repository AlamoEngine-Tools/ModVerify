using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting.Reporters.JSON;

internal class JsonReporter(JsonReporterSettings settings, IServiceProvider serviceProvider) : FileBasedReporter<JsonReporterSettings>(settings, serviceProvider)
{
    public const string FileName = "VerificationResult.json";


    public override async Task ReportAsync(IReadOnlyCollection<VerificationError> errors)
    {
        var report = new JsonVerificationReport(errors.Select(x => new JsonVerificationError(x)));
#if NET || NETSTANDARD2_1
        await 
#endif
        using var fs = CreateFile(FileName);
        await JsonSerializer.SerializeAsync(fs, report, ModVerifyJsonSettings.JsonSettings);
    }
}