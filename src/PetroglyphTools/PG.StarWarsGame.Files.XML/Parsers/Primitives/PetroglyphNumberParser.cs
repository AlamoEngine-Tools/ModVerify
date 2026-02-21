using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Utilities;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphNumberParser<T> : PetroglyphPrimitiveXmlParser<T> where T : struct, IEquatable<T>, IComparable<T>
#if NET10_0_OR_GREATER
    , INumber<T>, IMinMaxValue<T>
#endif
{

#if NET7_0_OR_GREATER
    protected virtual T MaxValue => T.MaxValue;
    
    protected virtual T MinValue => T.MinValue;
#else
    protected abstract T MaxValue { get; }
    
    protected abstract T MinValue { get; }

#endif

    public T ParseAtLeast(XElement element, T minValue)
    {
        if (minValue.CompareTo(MinValue) < 0 || minValue.CompareTo(MaxValue) > 0)
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue is out of range.");

        var value = Parse(element);
        var corrected = PGMath.Max(value, minValue);
        if (!corrected.Equals(value))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected value to be at least {minValue} but got value '{value}'.",
            });
        }

        return corrected;
    }

    public T ParseAtMost(XElement element, T maxValue)
    {
        if (maxValue.CompareTo(MinValue) < 0 || maxValue.CompareTo(MaxValue) > 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue is out of range.");

        var value = Parse(element);
        var corrected = PGMath.Min(value, maxValue);
        if (!corrected.Equals(value))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected value to be at least {maxValue} but got value '{value}'.",
            });
        }

        return corrected;
    }


    public T ParseClamped(XElement element, T minValue, T maxValue)
    {
        if (minValue.CompareTo(MinValue) < 0 || minValue.CompareTo(MaxValue) > 0)
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue is out of range.");
        if (maxValue.CompareTo(MinValue) < 0 || maxValue.CompareTo(MaxValue) > 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue is out of range.");
        if (minValue.CompareTo(maxValue) > 0)
            throw new ArgumentException("minValue must be less than or equal to maxValue.");

        var value = Parse(element);
        var clamped = PGMath.Clamp(value, minValue, maxValue);
        if (!value.Equals(clamped))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected integer between {minValue} and {maxValue} but got value '{value}'.",
            });
        }
        return clamped;
    }
}