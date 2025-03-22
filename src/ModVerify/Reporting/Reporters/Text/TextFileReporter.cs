using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters.Text;

internal class TextFileReporter(TextFileReporterSettings settings, IServiceProvider serviceProvider) : FileBasedReporter<TextFileReporterSettings>(settings, serviceProvider)
{
    internal const string SingleReportFileName = "VerificationResult.txt";
    
    public override async Task ReportAsync(IReadOnlyCollection<VerificationError> errors)
    {
        if (Settings.SplitIntoFiles)
            await ReportByVerifier(errors);
        else
            await ReportWhole(errors);
    }

    private async Task ReportWhole(IReadOnlyCollection<VerificationError> errors)
    {
#if NET || NETSTANDARD2_1
        await
#endif
        using var streamWriter = new StreamWriter(CreateFile(SingleReportFileName));

        foreach (var error in errors.OrderBy(x => x.Id))
            await WriteError(error, streamWriter);

    }

    private async Task ReportByVerifier(IReadOnlyCollection<VerificationError> errors)
    {
        var grouped = errors.GroupBy(x => x.VerifierChain.Last());
        foreach (var group in grouped) 
            await ReportToSingleFile(group);
    }

    private async Task ReportToSingleFile(IGrouping<string, VerificationError> group)
    {
        var fileName = $"{GetVerifierName(group.Key)}Results.txt";
#if NET || NETSTANDARD2_1
        await
#endif
        using var streamWriter = new StreamWriter(CreateFile(fileName));
        foreach (var error in group.OrderBy(x => x.Id)) 
            await WriteError(error, streamWriter);
    }

    private static string GetVerifierName(string verifierTypeName)
    {
        var typeNameSpan = verifierTypeName.AsSpan();
        var nameIndex = typeNameSpan.LastIndexOf('.');

        if (nameIndex == -1)
            return verifierTypeName;

        // They type name must not be empty
        if (typeNameSpan.Length == nameIndex)
            throw new InvalidOperationException();

        var name = typeNameSpan.Slice(nameIndex + 1);

        // Normalize subtypes (such as C/M) to avoid creating directories
        if (name.IndexOf('/') != -1)
            return name.ToString().Replace('/', '.');
        
        return name.ToString();
    }

    private static async Task WriteError(VerificationError error, StreamWriter writer)
    {
        await writer.WriteLineAsync($"[{error.Id}] {error.Message}");
    }
}