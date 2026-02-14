using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters;

internal sealed class TextFileReporter(TextFileReporterSettings settings, IServiceProvider serviceProvider) 
    : FileBasedReporter<TextFileReporterSettings>(settings, serviceProvider)
{
    internal const string SingleReportFileName = "VerificationResult.txt";
    
    public override async Task ReportAsync(VerificationResult verificationResult)
    {
        if (Settings.SplitIntoFiles)
            await ReportByVerifier(verificationResult);
        else
            await ReportWhole(verificationResult);
    }

    private async Task ReportWhole(VerificationResult result)
    {
#if NET || NETSTANDARD2_1
        await
#endif
        using var streamWriter = new StreamWriter(CreateFile(SingleReportFileName));

        await WriteHeader(result.Target, DateTime.Now, null, streamWriter);

        foreach (var error in result.Errors.OrderBy(x => x.Id))
            await WriteError(error, streamWriter);

    }

    private async Task ReportByVerifier(VerificationResult result)
    {
        var time = DateTime.Now;
        var grouped = result.Errors.GroupBy(x => x.VerifierChain.Last().Name);
        foreach (var group in grouped) 
            await ReportToSingleFile(group, result.Target, time);
    }

    private async Task ReportToSingleFile(IGrouping<string, VerificationError> group, VerificationTarget target, DateTime time)
    {
        var fileName = $"{GetVerifierName(group.Key)}_Results.txt";
#if NET || NETSTANDARD2_1
        await
#endif
        using var streamWriter = new StreamWriter(CreateFile(fileName));

        await WriteHeader(target, time, group.Key, streamWriter);

        foreach (var error in group.OrderBy(x => x.Id)) 
            await WriteError(error, streamWriter);
    }

    private static string GetVerifierName(string verifierTypeName)
    {
        var typeNameSpan = verifierTypeName.AsSpan();
        var nameIndex = typeNameSpan.LastIndexOf('.');
        var isSupType = typeNameSpan.IndexOf('/') > -1;

        if (nameIndex == -1 && !isSupType)
            return verifierTypeName;

        var name = typeNameSpan.Slice(nameIndex + 1);

        // Normalize subtypes (such as C/M) to avoid creating directories
        if (isSupType)
            return name.ToString().Replace('/', '.');
        
        return name.ToString();
    }

    private static async Task WriteHeader(
        VerificationTarget target,
        DateTime time,
        string? verifier,
        StreamWriter writer)
    {
        var header = CreateHeader(target, time, verifier);
        await writer.WriteLineAsync(header);
        await writer.WriteLineAsync();
    }

    private static string CreateHeader(VerificationTarget target, DateTime time, string? verifier)
    {
        var sb = new StringBuilder();
        sb.Append($"# Target '{target.Name}'");
        if (!string.IsNullOrEmpty(verifier))
            sb.Append($"; Verifier: {verifier}");
        sb.Append($"; Time: {time:s}");
        return sb.ToString();
    }

    private static async Task WriteError(VerificationError error, StreamWriter writer)
    {
        await writer.WriteLineAsync($"[{error.Id}] {error.Message}");
    }
}