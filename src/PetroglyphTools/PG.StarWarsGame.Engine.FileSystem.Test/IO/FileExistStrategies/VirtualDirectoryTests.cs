using System.Collections.Generic;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

public class VirtualDirectoryTests
{
    [Fact]
    public void Ctor_ExposesAssignedFields()
    {
        var files = new Dictionary<string, string> { ["foo.xml"] = "FOO.xml" };

        var dir = new VirtualDirectory("/some/dir", files);

        Assert.Equal("/some/dir", dir.OnDiskPath);
        Assert.Same(files, dir.Files);
        Assert.Equal("FOO.xml", dir.Files["foo.xml"]);
    }
}
