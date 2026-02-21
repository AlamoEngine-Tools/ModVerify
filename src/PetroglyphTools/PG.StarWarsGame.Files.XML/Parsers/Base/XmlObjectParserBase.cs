using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class XmlObjectParserBase<TObject, TParseState>(IXmlTagMapper<TObject> tagMapper, IXmlParserErrorReporter? errorReporter)
    : PetroglyphXmlParserBase(errorReporter) where TObject : XmlObject
{
    protected readonly IXmlTagMapper<TObject> XmlTagMapper = tagMapper ?? throw new ArgumentNullException(nameof(tagMapper));

    protected virtual bool IgnoreEmptyValue => true;

    protected virtual void ValidateAndFixupValues(TObject namedXmlObject, XElement element)
    {
    }

    protected void Parse(TObject xmlObject, XElement element, in TParseState state)
    {
        foreach (var tag in element.Elements())
        {
            if (!tag.HasElements)
            {
                if (string.IsNullOrEmpty(tag.PGValue) && IgnoreEmptyValue)
                    continue;

                if (!ParseTag(tag, xmlObject, state))
                {
                    ErrorReporter?.Report(new XmlError(this, tag)
                    {
                        Message = $"The node '{tag.Name}' is not supported.",
                        ErrorKind = XmlParseErrorKind.UnknownNode
                    });
                }
            }
            else
            {
                Parse(xmlObject, tag, in state);
            }
        }
    }

    protected virtual bool ParseTag(XElement tag, TObject xmlObject, in TParseState parseState)
    {
        return IsTagValid(tag) && XmlTagMapper.TryParseEntry(tag, xmlObject);
    }
}