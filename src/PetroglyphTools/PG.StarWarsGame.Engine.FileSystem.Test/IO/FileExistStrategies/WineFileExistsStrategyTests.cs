using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

public sealed class WineFileExistsStrategyTests : FileExistsStrategyTestBase
{
    protected override void ConfigureStrategy(PetroglyphFileSystem fs)
        => fs.UseWineStrategy();

    private protected override FileExistsStrategy CreateStrategyForCleanupTest()
        => new WineFileExistsStrategy(FileSystem);
}
