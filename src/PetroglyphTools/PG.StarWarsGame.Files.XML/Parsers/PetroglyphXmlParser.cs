using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParser<T> : IPetroglyphXmlParser<T>
{
    protected IServiceProvider ServiceProvider { get; }

    protected ILogger? Logger { get; }

    protected IPrimitiveParserProvider PrimitiveParserProvider { get; }

    protected PetroglyphXmlParser(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        PrimitiveParserProvider = serviceProvider.GetRequiredService<IPrimitiveParserProvider>();
    }

    public abstract T Parse(XElement element);

    object IPetroglyphXmlParser.Parse(XElement element)
    {
        return Parse(element);
    }
}