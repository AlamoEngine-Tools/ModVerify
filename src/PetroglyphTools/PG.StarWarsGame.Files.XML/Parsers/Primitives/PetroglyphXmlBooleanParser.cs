using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlBooleanParser : PetroglyphPrimitiveXmlParser<bool>
{
    public static readonly PetroglyphXmlBooleanParser Instance = new();

    private PetroglyphXmlBooleanParser()
    {
    }

    private protected override bool DefaultValue => false;

    protected internal override bool ParseCore(string trimmedValue, XElement element)
    {
        // Yes! The engine only checks if the values is exact 1 or starts with Tt or Yy
        // At least it's efficient, I guess...
        if (trimmedValue.Length == 1 && trimmedValue[0] == '1')
            return true;

        return trimmedValue[0] is 'y' or 'Y' or 't' or 'T';
    }
}