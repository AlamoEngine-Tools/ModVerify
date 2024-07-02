using System;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

internal class PrimitiveParserProvider(IServiceProvider serviceProvider) : IPrimitiveParserProvider
{
    private readonly Lazy<PetroglyphXmlStringParser> _lazyStringParser = new(() => new PetroglyphXmlStringParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlUnsignedIntegerParser> _lazyUintParser = new(() => new PetroglyphXmlUnsignedIntegerParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlLooseStringListParser> _lazyLooseStringListParser = new(() => new PetroglyphXmlLooseStringListParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlIntegerParser> _lazyIntParser = new(() => new PetroglyphXmlIntegerParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlFloatParser> _lazyFloatParser = new(() => new PetroglyphXmlFloatParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlByteParser> _lazyByteParser = new(() => new PetroglyphXmlByteParser(serviceProvider));
    private readonly Lazy<PetroglyphXmlBooleanParser> _lazyBoolParser = new(() => new PetroglyphXmlBooleanParser(serviceProvider));
    private readonly Lazy<CommaSeparatedStringKeyValueListParser> _lazyCommaStringKeyListParser = new(() => new CommaSeparatedStringKeyValueListParser(serviceProvider));

    public PetroglyphXmlStringParser StringParser => _lazyStringParser.Value;
    public PetroglyphXmlUnsignedIntegerParser UIntParser => _lazyUintParser.Value;
    public PetroglyphXmlLooseStringListParser LooseStringListParser => _lazyLooseStringListParser.Value;
    public PetroglyphXmlIntegerParser IntParser => _lazyIntParser.Value;
    public PetroglyphXmlFloatParser FloatParser => _lazyFloatParser.Value;
    public PetroglyphXmlByteParser ByteParser => _lazyByteParser.Value;
    public PetroglyphXmlBooleanParser BooleanParser => _lazyBoolParser.Value;
    public CommaSeparatedStringKeyValueListParser CommaSeparatedStringKeyValueListParser => _lazyCommaStringKeyListParser.Value;
}