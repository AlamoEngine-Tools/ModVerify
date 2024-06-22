using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public abstract class XmlObjectParser<T> : PetroglyphXmlElementParser<T> where T : XmlObject
{
    protected IServiceProvider ServiceProvider { get; }

    protected ILogger? Logger { get; }
    
    protected ICrc32HashingService HashingService { get; }

    protected XmlObjectParser(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    }

    protected abstract IPetroglyphXmlElementParser? GetParser(string tag);

    protected ValueListDictionary<string, object> ToKeyValuePairList(XElement element)
    {
        var keyValuePairList = new ValueListDictionary<string, object>();
        foreach (var elm in element.Elements())
        {
            var tagName = elm.Name.LocalName;

            var parser = GetParser(tagName);

            if (parser is not null)
            {
                var value = parser.Parse(elm);
                keyValuePairList.Add(tagName, value);
            }
        }
        return keyValuePairList;
    }
}