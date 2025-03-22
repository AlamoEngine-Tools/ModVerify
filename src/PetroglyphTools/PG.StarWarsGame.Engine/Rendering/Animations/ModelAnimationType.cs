using System;

namespace PG.StarWarsGame.Engine.Rendering.Animations;

public readonly struct ModelAnimationType : IEquatable<ModelAnimationType>
{
    public GameEngineType TargetEngine { get; }

    public int Value { get; }

    internal ModelAnimationType(GameEngineType engine, int value)
    {
        TargetEngine = engine;
        Value = value;
    }

    public override string ToString()
    {
        var nameLookup = SupportedModelAnimationTypes.GetAnimationTypesForEngine(TargetEngine);
        return $"'{nameLookup[this]}' ({Value})";
    }

    public bool Equals(ModelAnimationType other)
    {
        return TargetEngine == other.TargetEngine && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ModelAnimationType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)TargetEngine, Value);
    }

    public static bool operator ==(ModelAnimationType left, ModelAnimationType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ModelAnimationType left, ModelAnimationType right)
    {
        return !(left == right);
    }
}