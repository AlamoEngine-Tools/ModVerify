using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase.Environment;

namespace ModVerify.CliApp.Test.TestData;

internal class TestEnv(Assembly assembly, IFileSystem fileSystem) : ApplicationEnvironment(assembly, fileSystem)
{
    public override string ApplicationName => "TestEnv";
    protected override string ApplicationLocalDirectoryName => ApplicationName;
}