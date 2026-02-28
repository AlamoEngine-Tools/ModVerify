using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

// Used e.g, by <Land_Terrain_Model_Mapping>
// Format: Key, Value, Key, Value
// There might be arbitrary spaces, tabs and newlines

public sealed class CommaSeparatedStringKeyValueListParser : PetroglyphPrimitiveXmlParser<IList<(string key, string value)>>
{
    public static readonly CommaSeparatedStringKeyValueListParser Instance = new();

    private CommaSeparatedStringKeyValueListParser()
    {
    }

    private protected override IList<(string key, string value)> DefaultValue => [];

    internal override int EngineDataTypeId => 0x34;

    protected internal override IList<(string key, string value)> ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var valueText = element.PGValue;

        if (string.IsNullOrEmpty(valueText))
            return DefaultValue;

        if (valueText.Length >= 0x10000)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.TooLongData,
                Message = "Input string exceeds maximum size."
            });
            return DefaultValue;
        }

        var values = valueText.Split(',');

        // Cases: Empty tag or invalid value (e.g, terrain only, wrong separator, etc.)
        if (values.Length < 2)
            return DefaultValue;

        var keyValueList = new List<(string key, string value)>(values.Length + 1 / 2);

        for (var i = 0; i < values.Length; i += 2)
        {
            // Case: Incomplete key-value pair
            if (values.Length - 1 < i + 1)
            {
                ErrorReporter?.Report(new XmlError(this, element)
                { 
                    ErrorKind = XmlParseErrorKind.MalformedValue,
                    Message = "Unexpected end of string. Missing string for conversion/string pair!"
                });
                break;
            }

            var key = values[i].Trim();
            var value = values[i + 1].Trim();

            keyValueList.Add((key, value));
        }

        return keyValueList;
    }
}