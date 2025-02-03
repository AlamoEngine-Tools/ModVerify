using System;
using System.IO;
using System.IO.Abstractions;
using AET.ModVerify.Reporting.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Reporting.Reporters;

public abstract class FileBasedReporter<T>(T settings, IServiceProvider serviceProvider) : ReporterBase<T>(settings, serviceProvider) where T : FileBasedReporterSettings
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    protected Stream CreateFile(string fileName)
    {
        var outputDirectory = Settings.OutputDirectory;
        _fileSystem.Directory.CreateDirectory(outputDirectory);

        var filePath = _fileSystem.Path.Combine(outputDirectory, fileName);

        return _fileSystem.FileStream.New(_fileSystem.Path.GetFullPath(filePath), FileMode.Create, FileAccess.Write);
    }
}