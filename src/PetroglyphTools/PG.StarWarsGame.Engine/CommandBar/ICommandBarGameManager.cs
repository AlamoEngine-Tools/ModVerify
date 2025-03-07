using System.Collections.Generic;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files.MTD.Files;

namespace PG.StarWarsGame.Engine.CommandBar;

public interface ICommandBarGameManager : IGameManager<CommandBarBaseComponent>
{
    IMtdFile? MegaTextureFile { get; }

    ICollection<CommandBarBaseComponent> Components { get; }

    IReadOnlyDictionary<string, CommandBarComponentGroup> Groups { get; }
}