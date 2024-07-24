using System;

namespace PG.StarWarsGame.Engine.Utilities;

// From https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Text/ValueStringBuilder.cs
internal ref struct ValueStringBuilder(Span<char> initialBuffer)
{
    private readonly Span<char> _chars = initialBuffer;
    private int _pos = 0;

    public override string ToString()
    {
        return _chars.Slice(0, _pos).ToString();
    }

    public void Append(scoped ReadOnlySpan<char> value)
    {
        var pos = _pos;
        if (pos > _chars.Length - value.Length)
            throw new InvalidOperationException("Value string builder is too small.");

        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }
}