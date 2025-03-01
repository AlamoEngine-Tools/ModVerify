using System;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.Rendering;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public abstract class CommandBarBaseComponent(CommandBarComponentData xmlData)
{
    internal readonly CommandBarComponentData XmlData = xmlData ?? throw new ArgumentNullException(nameof(xmlData));
    
    public string Name => XmlData.Name;
    public RgbaColor Color { get; } = xmlData.Color.HasValue ? new RgbaColor(xmlData.Color.Value) : new RgbaColor(255, 255, 255, 255);
    public int Bone { get; internal set; } = -1;
    public int BaseLayer { get; } = xmlData.BaseLayer;
    public bool Hidden { get; internal set; } = xmlData.Hidden;
    public bool Disabled { get; } = xmlData.Disabled;
    public abstract CommandBarComponentType Type { get; }
    public CommandBarComponentId Id { get; internal set; }
    public CommandBarComponentGroup? Group { get; internal set; }
    public CommandBarShellComponent? ParentShell { get; internal set; }

    public static CommandBarBaseComponent? Create(CommandBarComponentData xmlData, IGameErrorReporter errorReporter)
    {
        var type = GetTypeFromString(xmlData.Type.AsSpan());
        switch (type)
        {
            case CommandBarComponentType.Shell:
                return new CommandBarShellComponent(xmlData);
            case CommandBarComponentType.Icon:
                return new CommandBarIconComponent(xmlData);
            case CommandBarComponentType.Button:
                return new CommandBarButtonComponentClass(xmlData);
            case CommandBarComponentType.Text:
                return new CommandBarTextComponent(xmlData);
            case CommandBarComponentType.TextButton:
                return new CommandBarTextButtonComponent(xmlData);
            case CommandBarComponentType.Model:
                return new CommandBarModelComponent(xmlData);
            case CommandBarComponentType.Bar:
                return new CommandBarBarComponent(xmlData);
            case CommandBarComponentType.Select:
                // I don't know how this code could ever be reached, because GetTypeFromString,
                // if I understand it correctly, does not allow SELECT (and COUNT). The code to create a 
                // SELECT Component exists nether the less.
                return new CommandBarSelectComponent(xmlData);
        }

        // TODO: Verifier for any invalid type value
        errorReporter.Assert(
            EngineAssert.Create(EngineAssertKind.InvalidValue, type, xmlData.Name,
            $"Invalid type value '{xmlData.Type}' for CommandbarComponent '{xmlData.Name}')"));

        return null;
    }

    public override string ToString()
    {
        return Name;
    }

    private static CommandBarComponentType GetTypeFromString(ReadOnlySpan<char> xmlValue)
    {
        if (xmlValue.Equals("SHELL".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Shell;
        if (xmlValue.Equals("ICON".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Icon;
        if (xmlValue.Equals("BUTTON".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Button;
        if (xmlValue.Equals("TEXT".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Text;
        if (xmlValue.Equals("TEXTBUTTON".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.TextButton;
        if (xmlValue.Equals("MODEL".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Model;
        if (xmlValue.Equals("BAR".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return CommandBarComponentType.Bar;
        return CommandBarComponentType.None;
    }
}