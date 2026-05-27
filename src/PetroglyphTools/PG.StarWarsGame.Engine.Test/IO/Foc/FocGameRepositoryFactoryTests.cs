using PG.StarWarsGame.Engine.IO.Repositories;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO.Foc;

public class FocGameRepositoryFactoryTests : GameRepositoryFactoryTests
{
    protected override GameEngineType Engine => GameEngineType.Foc;

    [Fact]
    public void Create_ReturnsFocGameRepository()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.IsType<FocGameRepository>(gameRepo);
    }
}
