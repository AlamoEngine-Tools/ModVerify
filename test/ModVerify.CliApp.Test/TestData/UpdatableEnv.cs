using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase.Environment;
using AnakinRaW.AppUpdaterFramework.Configuration;

namespace ModVerify.CliApp.Test.TestData;

internal class UpdatableEnv(Assembly assembly, IFileSystem fileSystem) : UpdatableApplicationEnvironment(assembly, fileSystem)
{
    public override string ApplicationName => "TestUpdateEnv";
    protected override string ApplicationLocalDirectoryName => ApplicationName;
    public override ICollection<Uri> UpdateMirrors => [];
    public override string UpdateRegistryPath => ApplicationName;
    protected override UpdateConfiguration CreateUpdateConfiguration()
    {
        return UpdateConfiguration.Default;
    }
}