using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Reporting.Reporters.Text;

internal class TextFileReporter(TextFileReporterSettings settings, IServiceProvider serviceProvider) : FileBasedReporter<TextFileReporterSettings>(settings, serviceProvider)
{
    internal const string SingleReportFileName = "VerificationResult.txt";

    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public override void Report(IReadOnlyCollection<VerificationError> errors)
    {
        if (Settings.SplitIntoFiles)
            ReportByVerifier(errors);
        else
            ReportWhole(errors);
    }

    private void ReportWhole(IReadOnlyCollection<VerificationError> errors)
    {
        using var streamWriter = new StreamWriter(CreateFile(SingleReportFileName));
        
        foreach (var error in errors.OrderBy(x => x.Id)) 
            WriteError(error, streamWriter);
            
    }

    private void ReportByVerifier(IReadOnlyCollection<VerificationError> errors)
    {
        var grouped = errors.GroupBy(x => x.Verifier);
        foreach (var group in grouped) 
            ReportToSingleFile(group);
    }

    private void ReportToSingleFile(IGrouping<string, VerificationError> group)
    {
        var fileName = $"{GetVerifierName(group.Key)}Results.txt";
        using var streamWriter = new StreamWriter(CreateFile(fileName));
        foreach (var error in group.OrderBy(x => x.Id)) 
            WriteError(error, streamWriter);
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

    private static void WriteError(VerificationError error, StreamWriter writer)
    {
        writer.WriteLine($"[{error.Id}] {error.Message}");
    }
}