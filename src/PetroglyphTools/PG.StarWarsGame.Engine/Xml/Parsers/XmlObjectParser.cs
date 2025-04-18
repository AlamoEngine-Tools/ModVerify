﻿using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public abstract class XmlObjectParser<TObject>(
    IReadOnlyValueListDictionary<Crc32, TObject> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParser<TObject, EmptyParseState>(parsedElements, serviceProvider, errorReporter) where TObject : XmlObject
{
    protected void Parse(TObject xmlObject, XElement element)
    {
        Parse(xmlObject, element, EmptyParseState.Instance);
    }

    protected sealed override bool ParseTag(XElement tag, TObject xmlObject, in EmptyParseState parseState)
    {
        return ParseTag(tag, xmlObject);
    }

    protected abstract bool ParseTag(XElement tag, TObject xmlObject);
}

public readonly struct EmptyParseState
{
    public static readonly EmptyParseState Instance = new();
}


public abstract class XmlObjectParser<TObject, TParseState>(
    IReadOnlyValueListDictionary<Crc32, TObject> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : PetroglyphXmlElementParser<TObject>(errorReporter) where TObject : XmlObject
{
    protected IReadOnlyValueListDictionary<Crc32, TObject> ParsedElements { get; } =
        parsedElements ?? throw new ArgumentNullException(nameof(parsedElements));

    protected ICrc32HashingService HashingService { get; } = serviceProvider.GetRequiredService<ICrc32HashingService>();

    public abstract TObject Parse(XElement element, out Crc32 crc32);

    protected void Parse(TObject xmlObject, XElement element, in TParseState state)
    {
        foreach (var tag in element.Elements())
        {
            if (!ParseTag(tag, xmlObject, state))
            {
                OnParseError(new XmlParseErrorEventArgs(tag, XmlParseErrorKind.UnknownNode,
                    $"The node '{tag.Name}' is not supported."));
                break;
            }
        }
    }

    protected abstract bool ParseTag(XElement tag, TObject xmlObject, in TParseState parseState);

    protected string GetXmlObjectName(XElement element, out Crc32 crc32, bool uppercaseName)
    {
        GetNameAttributeValue(element, out var name);
        crc32 = uppercaseName
            ? HashingService.GetCrc32Upper(name.AsSpan(), PGConstants.DefaultPGEncoding)
            : HashingService.GetCrc32(name.AsSpan(), PGConstants.DefaultPGEncoding);

        if (crc32 == default)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Name for XmlObject cannot be empty."));
        }

        return name;
    }
}