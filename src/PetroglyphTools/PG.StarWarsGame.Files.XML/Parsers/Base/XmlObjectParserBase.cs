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

    protected virtual void ValidateAndFixupValues(TObject namedXmlObject, XElement element, in TParseState state)
    {
    }

    protected virtual void ParseObject(TObject xmlObject, XElement element, bool replace, in TParseState state)
    {
        ParseTags(xmlObject, element, replace, in state);
    }

    protected virtual bool ParseTag(XElement tag, TObject xmlObject, bool replace, in TParseState parseState)
    {
        return IsTagValid(tag) && XmlTagMapper.TryParseEntry(tag, xmlObject, replace);
    }

    protected void ParseTags(TObject xmlObject, XElement element, bool replace, in TParseState state)
    {
        foreach (var tag in element.Elements())
        {
            if (!tag.HasElements)
            {
                if (string.IsNullOrEmpty(tag.PGValue) && IgnoreEmptyValue)
                    continue;

                if (!ParseTag(tag, xmlObject, replace, state))
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
                ParseObject(xmlObject, tag, replace, in state);
            }
        }
    }
}