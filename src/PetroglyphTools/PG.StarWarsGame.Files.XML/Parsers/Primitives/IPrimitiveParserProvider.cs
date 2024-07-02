namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public interface IPrimitiveParserProvider
{
    PetroglyphXmlStringParser StringParser { get; }

    PetroglyphXmlUnsignedIntegerParser  UIntParser { get; }

    PetroglyphXmlLooseStringListParser LooseStringListParser { get; }

    PetroglyphXmlIntegerParser IntParser { get; }

    PetroglyphXmlFloatParser FloatParser { get; }

    PetroglyphXmlByteParser ByteParser { get; }

    PetroglyphXmlBooleanParser BooleanParser { get; }

    CommaSeparatedStringKeyValueListParser CommaSeparatedStringKeyValueListParser { get; }
}