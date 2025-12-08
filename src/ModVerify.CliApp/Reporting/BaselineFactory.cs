using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.App.Reporting;

internal sealed class BaselineFactory(IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(BaselineFactory));
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public bool TryCreateBaseline(
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

    public VerificationBaseline CreateBaseline(string filePath)
    { 
        return CreateBaselineFromFilePath(filePath);
    }

    private VerificationBaseline CreateBaselineFromFilePath(string baselineFile)
    {
        using var fs = _fileSystem.FileStream.New(baselineFile, FileMode.Open, FileAccess.Read);
        return VerificationBaseline.FromJson(fs);
    }
}