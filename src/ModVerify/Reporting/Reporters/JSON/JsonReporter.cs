using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting.Reporters.JSON;

internal class JsonReporter(JsonReporterSettings settings, IServiceProvider serviceProvider) : FileBasedReporter<JsonReporterSettings>(settings, serviceProvider)
{
    public const string FileName = "VerificationResult.json";


    public override void Report(IReadOnlyCollection<VerificationError> errors)
    {
        var report = new JsonVerificationReport(errors.Select(x => new JsonVerificationError(x)));
        using var fs = CreateFile(FileName);
        JsonSerializer.Serialize(fs, report, ModVerifyJsonSettings.JsonSettings);
    }
}