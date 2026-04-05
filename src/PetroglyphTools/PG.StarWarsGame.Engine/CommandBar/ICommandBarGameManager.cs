using System.Collections.Generic;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.MTD.Files;

namespace PG.StarWarsGame.Engine.CommandBar;

public interface ICommandBarGameManager : IGameManager<CommandBarBaseComponent>
{
    IMtdFile? MtdFile { get; }

    string? MegaTextureFileName { get; }

    ICollection<CommandBarBaseComponent> Components { get; }

    IReadOnlyDictionary<string, CommandBarComponentGroup> Groups { get; }

    bool IconExists(GameObjectType gameObjectType);
}