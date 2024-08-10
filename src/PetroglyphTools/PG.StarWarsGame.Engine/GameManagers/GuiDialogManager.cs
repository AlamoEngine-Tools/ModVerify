using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Files.MTD.Files;

namespace PG.StarWarsGame.Engine.GameManagers;

public interface IGameManager
{
    Task InitializeAsync(CancellationToken token);
}

internal abstract class GameManagerBase : IGameManager
{
    public Task InitializeAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}

public interface IGuiDialogManager : IGameManager
{
    IMtdFile MegaTexture { get; }
}

internal class GuiDialogManager : GameManagerBase
{
    public IMtdFile MegaTexture { get; }
}