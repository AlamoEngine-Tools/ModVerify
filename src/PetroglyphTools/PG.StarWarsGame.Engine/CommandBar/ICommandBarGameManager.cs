using System.Collections.Generic;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.Database;

namespace PG.StarWarsGame.Engine.CommandBar;

public interface ICommandBarGameManager : IGameManager<CommandBarBaseComponent>
{
    ICollection<CommandBarBaseComponent> Components { get; }
}