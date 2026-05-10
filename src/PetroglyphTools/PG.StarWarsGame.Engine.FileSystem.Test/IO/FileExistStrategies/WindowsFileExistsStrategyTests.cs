using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.IO;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

/// <summary>
/// Inherits the strategy-agnostic suite. The Windows strategy delegates to <c>CreateFileA</c>,
/// so the OS resolves casing — the buffer goes through unmodified, which is the looser
/// "case-insensitive equality" assertion in the base class.
/// </summary>
public sealed class WindowsFileExistsStrategyTests : FileExistsStrategyTestBase
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Assert.Skip("Windows strategy requires a Windows host.");
        fs.UseWindowsStrategy();
    }
}
