using PG.Commons.Numerics;
using System;

namespace PG.StarWarsGame.Engine.Rendering;

public readonly struct RgbaColor(float r, float g, float b, float a) : IEquatable<RgbaColor>
{
    private readonly float _r = r;
    private readonly float _g = g;
    private readonly float _b = b;
    private readonly float _a = a;

    public uint R => unchecked((uint)(_r * 255.0f));
    public uint G => unchecked((uint)(_g * 255.0f));
    public uint B => unchecked((uint)(_b * 255.0f));
    public uint A => unchecked((uint)(_a * 255.0f));

    public float Rf => _r;
    public float Gf => _g;
    public float Bf => _b;
    public float Af => _a;
    
    public RgbaColor() : this(1.0f, 1.0f, 1.0f, 1.0f)
    {
    }

    public RgbaColor(Vector4Int rgbaVector) : this(
        unchecked((byte)rgbaVector.First), 
        unchecked ((byte)rgbaVector.Second),
        unchecked ((byte)rgbaVector.Third), 
        unchecked ((byte)rgbaVector.Fourth))
    {
    }

    public RgbaColor(uint r, uint g, uint b, uint a) : this(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f)
    {
    }

    public override string ToString()
    {
        return $"{nameof(RgbaColor)} [R={R}, G={G}, B={B}, A={A}]";
    }
    
    public RgbaColor Lerp(RgbaColor a, RgbaColor b, float t)
    {
        var red = a.Rf + (b.Rf - a.Rf) * t;
        var green = a.Gf + (b.Gf - a.Gf) * t;
        var blue = a.Bf + (b.Bf - a.Bf) * t;
        var alpha = a.Af + (b.Af - a.Af) * t;
        return new RgbaColor(red, green, blue, alpha);
    }

    public bool Equals(RgbaColor other)
    {
        return _r.Equals(other._r) && _g.Equals(other._g) && _b.Equals(other._b) && _a.Equals(other._a);
    }

    public override bool Equals(object? obj)
    {
        return obj is RgbaColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_r, _g, _b, _a);
    }
}