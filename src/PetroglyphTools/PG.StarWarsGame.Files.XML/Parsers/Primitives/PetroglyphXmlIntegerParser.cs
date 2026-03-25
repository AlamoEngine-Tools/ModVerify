using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlIntegerParser : PetroglyphNumberParser<int>
{
    public static readonly PetroglyphXmlIntegerParser Instance = new();

    private protected override int DefaultValue => 0;

    internal override int EngineDataTypeId => 0x6;

#if !NET7_0_OR_GREATER
    protected override int MaxValue => int.MaxValue;

    protected override int MinValue => int.MinValue;
#endif

    private PetroglyphXmlIntegerParser()
    {
    }

    protected internal override int ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        // The engines uses the C++ function std::atoi which is a little more loose.
        // For example the value '123d' get parsed to 123,
        // whereas in C# int.TryParse returns (false, 0)
        if (!int.TryParse(trimmedValue
#if NETSTANDARD2_0
                    .ToString()
#endif

                , out var i))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.MalformedValue,
                Message = $"Expected integer but got '{trimmedValue.ToString()}'.",
            });
            return DefaultValue;
        }

        return i;
    }
}