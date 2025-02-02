﻿using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System.Runtime.CompilerServices;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlIntegerParser : PetroglyphXmlPrimitiveElementParser<int>
{ 
    internal PetroglyphXmlIntegerParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override int Parse(XElement element)
    {
        // The engines uses the C++ function std::atoi which is a little more loose.
        // For example the value '123d' get parsed to 123,
        // whereas in C# int.TryParse returns (false, 0)

        if (!int.TryParse(element.Value, out var i))
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.MalformedValue,
                $"Expected integer but got '{element.Value}'."));
            return 0;
        }

        return i;
    }

    public int ParseWithRange(XElement element, int minValue, int maxValue)
    {
        var value = Parse(element);
        var clamped =  PGMath.Clamp(value, minValue, maxValue);
        if (value != clamped)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected integer between {minValue} and {maxValue} but got value '{value}'."));
        }
        return clamped;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}

internal static class PGMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        if (min > max)
            throw new ArgumentException("min cannot be larger than max.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(byte value, byte min, byte max)
    {
        if (min > max)
            throw new ArgumentException("min cannot be larger than max.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }
}

