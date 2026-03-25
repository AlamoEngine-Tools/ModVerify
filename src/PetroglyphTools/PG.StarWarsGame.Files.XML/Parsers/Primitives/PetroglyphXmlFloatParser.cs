using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlFloatParser : PetroglyphNumberParser<float>
{
    public static readonly PetroglyphXmlFloatParser Instance = new();

    private protected override float DefaultValue => 0.0f;

    internal override int EngineDataTypeId => 0x8;

#if !NET7_0_OR_GREATER
    protected override float MaxValue => float.MaxValue;

    protected override float MinValue => float.MinValue;
#endif

    private PetroglyphXmlFloatParser()
    {
    }
    
    protected internal override float ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(trimmedValue
#if NETSTANDARD2_0
                    .ToString()
#endif
                , NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.MalformedValue,
                Message = $"Expected double but got value '{trimmedValue.ToString()}'.",
            });
            return 0.0f;
        }

        return (float)doubleValue;
    }
}