using System.Collections.Generic;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

// Used e.g, by <Land_Terrain_Model_Mapping>
// Format: Key, Value, Key, Value
// There might be arbitrary spaces, tabs and newlines
// TODO: This class is not yet implemented, compliant to the engine
public sealed class CommaSeparatedStringKeyValueListParser : PetroglyphPrimitiveXmlParser<IList<(string key, string value)>>
{
    public static readonly CommaSeparatedStringKeyValueListParser Instance = new();

    private CommaSeparatedStringKeyValueListParser()
    {
    }

    public override IList<(string key, string value)> Parse(XElement element)
    {
        var values = element.Value.Split(',');

        // Cases: Empty tag or invalid value (e.g, terrain only, wrong separator, etc.)
        if (values.Length <= 1)
            return new List<(string key, string value)>(0);

        var keyValueList = new List<(string key, string value)>(values.Length + 1 / 2);

        for (var i = 0; i < values.Length; i += 2)
        {
            // Case: Incomplete key-value pair
            if (values.Length - 1 < i + 1)
                break;

            var key = values[i].Trim();
            var value = values[i + 1].Trim();

            keyValueList.Add((key, value));
        }

        return keyValueList;
    }
}