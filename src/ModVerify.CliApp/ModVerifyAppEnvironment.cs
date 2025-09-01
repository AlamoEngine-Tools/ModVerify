using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase.Environment;
#if !NET
using System;
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
        new("C:\\Test\\ModVerify"),    
#endif
        new($"https://republicatwar.com/downloads/{ModVerifyConstants.ModVerifyToolPath}")
    };

    public override string UpdateRegistryPath => $@"SOFTWARE\{ModVerifyConstants.ModVerifyToolPath}\Update";
    
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