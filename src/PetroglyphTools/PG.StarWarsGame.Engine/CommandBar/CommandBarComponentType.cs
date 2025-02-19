namespace PG.StarWarsGame.Engine.CommandBar;

public enum CommandBarComponentType
{
    // Used for XML lookup
    Shell = 0,
    Icon = 1,
    Button = 2,
    Text = 3,
    TextButton = 4,
    Model = 5,
    Bar = 6,
    // Used internally by the engine only
    Select = 7,
    Count = 8,
    None = 9,
}