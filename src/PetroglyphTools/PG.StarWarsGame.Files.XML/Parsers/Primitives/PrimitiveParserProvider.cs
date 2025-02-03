using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

internal class PrimitiveParserProvider(IServiceProvider serviceProvider) : IPrimitiveParserProvider
{
    private readonly IPrimitiveXmlParserErrorListener _primitiveParserErrorListener = serviceProvider.GetRequiredService<IPrimitiveXmlParserErrorListener>();

    private PetroglyphXmlStringParser _stringParser = null!;
    private PetroglyphXmlUnsignedIntegerParser _uintParser = null!;
    private PetroglyphXmlLooseStringListParser _looseStringListParser = null!;
    private PetroglyphXmlIntegerParser _intParser = null!;
    private PetroglyphXmlFloatParser _floatParser = null!;
    private PetroglyphXmlByteParser _byteParser = null!;
    private PetroglyphXmlMax100ByteParser _max100ByteParser = null!;
    private PetroglyphXmlBooleanParser _booleanParser = null!;
    private PetroglyphXmlVector2FParser _vector2FParser = null!;

    private CommaSeparatedStringKeyValueListParser _commaSeparatedStringKeyValueListParser = null!;

    public PetroglyphXmlStringParser StringParser => 
        LazyInitializer.EnsureInitialized(ref _stringParser, () => new PetroglyphXmlStringParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlUnsignedIntegerParser UIntParser => 
        LazyInitializer.EnsureInitialized(ref _uintParser, () => new PetroglyphXmlUnsignedIntegerParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlLooseStringListParser LooseStringListParser => 
        LazyInitializer.EnsureInitialized(ref _looseStringListParser, () => new PetroglyphXmlLooseStringListParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlIntegerParser IntParser => 
        LazyInitializer.EnsureInitialized(ref _intParser, () => new PetroglyphXmlIntegerParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlFloatParser FloatParser => 
        LazyInitializer.EnsureInitialized(ref _floatParser, () => new PetroglyphXmlFloatParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlByteParser ByteParser => 
        LazyInitializer.EnsureInitialized(ref _byteParser, () => new PetroglyphXmlByteParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlMax100ByteParser Max100ByteParser => 
        LazyInitializer.EnsureInitialized(ref _max100ByteParser, () => new PetroglyphXmlMax100ByteParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlBooleanParser BooleanParser => 
        LazyInitializer.EnsureInitialized(ref _booleanParser, () => new PetroglyphXmlBooleanParser(serviceProvider, _primitiveParserErrorListener));
    
    public PetroglyphXmlVector2FParser Vector2FParser => 
        LazyInitializer.EnsureInitialized(ref _vector2FParser, () => new PetroglyphXmlVector2FParser(serviceProvider, _primitiveParserErrorListener));
    
    public CommaSeparatedStringKeyValueListParser CommaSeparatedStringKeyValueListParser => 
        LazyInitializer.EnsureInitialized(ref _commaSeparatedStringKeyValueListParser, () => new CommaSeparatedStringKeyValueListParser(serviceProvider, _primitiveParserErrorListener));
}