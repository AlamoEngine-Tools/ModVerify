using PG.StarWarsGame.Engine.CommandBar.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Engine.CommandBar;

public sealed class CommandBarComponentGroup
{
    public string Name { get; }

    public IReadOnlyList<CommandBarBaseComponent> Components { get; }

    internal CommandBarComponentGroup(string name, IEnumerable<CommandBarBaseComponent> components)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        if (components == null)
            throw new ArgumentNullException(nameof(components));
        Components = components.ToList();
    }
}