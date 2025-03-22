using System;

namespace PG.StarWarsGame.Engine.CommandBar;

public readonly struct CommandBarComponentId : IEquatable<CommandBarComponentId>
{
    public static readonly CommandBarComponentId None = new(GameEngineType.Eaw, -1);

    public GameEngineType TargetEngine { get; }

    public int Value { get; }

    internal CommandBarComponentId(GameEngineType engine, int value)
    {
        TargetEngine = engine;
        Value = value;
    }

    public override string ToString()
    {
        if (Value == -1)
            return "NONE (-1)";

        var nameLookup = SupportedCommandBarComponentData.GetComponentIdsForEngine(TargetEngine);
        return $"'{nameLookup[this]}' ({Value})";
    }

    public bool Equals(CommandBarComponentId other)
    {
        return TargetEngine == other.TargetEngine && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is CommandBarComponentId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)TargetEngine, Value);
    }

    public static bool operator ==(CommandBarComponentId left, CommandBarComponentId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CommandBarComponentId left, CommandBarComponentId right)
    {
        return !(left == right);
    }
}