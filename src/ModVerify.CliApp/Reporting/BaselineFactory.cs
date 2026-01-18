using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace AET.ModVerify.App.Reporting;

internal sealed class BaselineFactory(IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(BaselineFactory));
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public bool TryFindBaselineInDirectory(
        string directory,
        out VerificationBaseline baseline,
        [NotNullWhen(true)] out string? path)
    {
        baseline = VerificationBaseline.Empty;
        path = null;

        if (!_fileSystem.Directory.Exists(directory))
            return false;

        _logger?.LogDebug(ModVerifyConstants.ConsoleEventId, "Searching for baseline file at '{Directory}'", directory);

        var jsonFiles = _fileSystem.Directory.EnumerateFiles(
            directory,
            "*.json"
#if NET || NETSTANDARD2_1_OR_GREATER
            , new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = false
            }
#endif
        );

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                baseline = CreateBaselineFromFilePath(jsonFile);
                path = jsonFile;
                _logger?.LogDebug("Create baseline from file: {JsonFile}", jsonFile);
                return true;
            }
            catch (InvalidBaselineException e)
            {
                _logger?.LogDebug("'{JsonFile}' is not a valid baseline file: {Message}", jsonFile, e.Message);
                // Ignore this exception
            }
        }

        path = null;
        return false;
    }

    public VerificationBaseline ParseBaseline(string filePath)
    { 
        return CreateBaselineFromFilePath(filePath);
    }

    public VerificationBaseline CreateBaseline(
        VerificationTarget target, 
        GlobalVerifyReportSettings reportSettings, 
        IEnumerable<VerificationError> errors)
    {
        return new VerificationBaseline(reportSettings.MinimumReportSeverity, errors, target);
    }

    public async Task WriteBaselineAsync(VerificationBaseline baseline, string filePath)
    {
        var fullPath = _fileSystem.Path.GetFullPath(filePath);
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Writing Baseline to '{FullPath}'", fullPath);

#if NET
        await
#endif
            using var fs = _fileSystem.FileStream.New(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await baseline.ToJsonAsync(fs);
    }

    private VerificationBaseline CreateBaselineFromFilePath(string baselineFile)
    {
        using var fs = _fileSystem.FileStream.New(baselineFile, FileMode.Open, FileAccess.Read);
        return VerificationBaseline.FromJson(fs);
    }
}