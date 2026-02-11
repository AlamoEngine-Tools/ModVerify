using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase.Environment;
#if !NET
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.CommonUtilities.DownloadManager.Configuration;
#endif

namespace AET.ModVerify.App;

internal sealed class ModVerifyAppEnvironment(Assembly assembly, IFileSystem fileSystem)
#if NET
    : ApplicationEnvironment(assembly, fileSystem)
#else
    : UpdatableApplicationEnvironment(assembly, fileSystem)
#endif
{
    public override string ApplicationName => ModVerifyConstants.AppNameString;

    protected override string ApplicationLocalDirectoryName => ModVerifyConstants.ModVerifyToolPath;

#if NETFRAMEWORK

    public override ICollection<Uri> UpdateMirrors { get; } = new List<Uri>
    {
#if DEBUG
        new(CreateDebugPath()),    
#endif
        new($"https://republicatwar.com/downloads/{ModVerifyConstants.ModVerifyToolPath}")
    };

    private static string CreateDebugPath()
    {
        var dir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../.."));
        return Path.Combine(dir, ".local_deploy/server");
    }

    public override string UpdateRegistryPath => $@"SOFTWARE\{ModVerifyConstants.ModVerifyToolPath}\Update";

#if NETFRAMEWORK
    static ModVerifyAppEnvironment()
    {
        // For some unknown reason, packaging dependencies into the app, may alter the used security protocols...
        // This reverts the changes and forces secure settings
        if (ServicePointManager.SecurityProtocol != SecurityProtocolType.SystemDefault)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls12;
    }
#endif

    protected override UpdateConfiguration CreateUpdateConfiguration()
    { 
        return new UpdateConfiguration
        {
            DownloadLocation = FileSystem.Path.Combine(ApplicationLocalPath, "downloads"),
            BackupLocation = FileSystem.Path.Combine(ApplicationLocalPath, "backups"),
            BackupPolicy = BackupPolicy.Required,
            ComponentDownloadConfiguration = new DownloadManagerConfiguration
            {
                AllowEmptyFileDownload = false,
                DownloadRetryDelay = 500,
                ValidationPolicy = ValidationPolicy.Required
            },
            ManifestDownloadConfiguration = new DownloadManagerConfiguration
            {
                AllowEmptyFileDownload = false,
                DownloadRetryDelay = 500,
                ValidationPolicy = ValidationPolicy.Optional
            },
            BranchDownloadConfiguration = new DownloadManagerConfiguration
            {
                AllowEmptyFileDownload = false,
                DownloadRetryDelay = 500,
                ValidationPolicy = ValidationPolicy.NoValidation
            },
            DownloadRetryCount = 3,
            RestartConfiguration = new UpdateRestartConfiguration
            {
                SupportsRestart = true,
                PassCurrentArgumentsForRestart = true
            },
            ValidateInstallation = true
        };
    }
#endif
}