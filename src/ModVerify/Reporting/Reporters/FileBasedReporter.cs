using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides a base class for verification reporters that write their output to files.</summary>
/// <typeparam name="T">The type of settings used by the reporter.</typeparam>
/// <param name="settings">The settings that control the reporter's behavior.</param>
/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
public abstract class FileBasedReporter<T>(T settings, IServiceProvider serviceProvider)
    : ReporterBase<T>(settings, serviceProvider) where T : FileBasedReporterSettings
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    /// <summary>Creates a writable file stream for the specified file name in the configured output directory.</summary>
    /// <param name="fileName">The name of the file to create.</param>
    /// <returns>A writable stream for the newly created file.</returns>
    protected Stream CreateFile(string fileName)
    {
        var outputDirectory = Settings.OutputDirectory;
        _fileSystem.Directory.CreateDirectory(outputDirectory);

        var filePath = _fileSystem.Path.Combine(outputDirectory, fileName);

        return _fileSystem.FileStream.New(_fileSystem.Path.GetFullPath(filePath), FileMode.Create, FileAccess.Write);
    }
}