using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

public sealed class WineFileExistsStrategyTests : FileExistsStrategyTestBase
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
        => fs.UseWineStrategy();
}
