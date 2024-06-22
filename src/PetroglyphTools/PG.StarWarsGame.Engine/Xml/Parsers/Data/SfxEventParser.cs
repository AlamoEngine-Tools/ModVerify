using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class SfxEventParser(IServiceProvider serviceProvider) : XmlObjectParser<SfxEvent>(serviceProvider)
{
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SfxEventParser));

    public override SfxEvent Parse(
        XElement element, 
        IReadOnlyValueListDictionary<Crc32, SfxEvent> parsedElements, 
        out Crc32 nameCrc)
    {
        var name = GetNameAttributeValue(element);
        nameCrc = _hashingService.GetCrc32Upper(name.AsSpan(), PGConstants.PGCrc32Encoding);

        var valueList = new ValueListDictionary<string, object?>();

        foreach (var child in element.Elements())
        {
            var tagName = child.Name.LocalName;
            var parser = GetParser(tagName);
            if (parser is null)
            {
                //_logger?.LogWarning($"Unable to find parser for tag '{tagName}' in element '{name}'");
                continue;
            }

            if (tagName.Equals("Use_Preset"))
            {
                var presetName = parser.Parse(child) as string;
                var presetNameCrc = _hashingService.GetCrc32Upper(presetName.AsSpan(), PGConstants.PGCrc32Encoding);
                if (presetNameCrc == default || !parsedElements.TryGetFirstValue(presetNameCrc, out var preset))
                {
                    // Unable to find Preset
                    continue;
                }
            }
            else
            {
                valueList.Add(tagName, parser.Parse(child));
            }
        }

        var sfxEvent = new SfxEvent(name, nameCrc, XmlLocationInfo.FromElement(element));

        return sfxEvent;
    }

    public override SfxEvent Parse(XElement element)
    {
        throw new NotSupportedException();
    }

    protected override bool IsTagSupported(string tag)
    {
        throw new NotImplementedException();
    }
}