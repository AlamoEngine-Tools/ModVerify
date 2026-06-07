using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Reporting.Engine;

internal sealed class XmlParseErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider) :
    EngineErrorReporterBase<XmlError>(gameRepository, serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public override string FriendlyName => "XML Errors";

    protected override ErrorData CreateError(XmlError error)
    {
        var descriptor = GetDescriptor(error.ErrorKind);

        var strippedFileName = _fileSystem.Path
            .GetGameStrippedPath(GameRepository.Path.AsSpan(), error.FileLocation.XmlFile.ToUpperInvariant().AsSpan()).ToString();

        var asset = strippedFileName;
        
        var context = new List<string>
        {
            $"Parser: {error.Parser.Name}",
            $"File: {strippedFileName}" 
        };

        var xmlElement = error.Element;

        if (xmlElement is not null)
        {
            var parent = xmlElement.Parent;

            if (parent != null)
            {
                var parentName = parent.Attribute("Name");
                context.Add(parentName != null ? $"{parentName.Value}" : $"<{parent.Name.LocalName}>");
            }

            var localName = xmlElement.Name.LocalName;
            asset = localName;
        }

        var errorMessage = CreateErrorMessage(error, strippedFileName);
        return new ErrorData(descriptor.Id, errorMessage, context, asset, descriptor.Severity);
    }

    private static string CreateErrorMessage(XmlError error, string strippedFileName)
    {
        if (error.FileLocation.Line.HasValue)
            return $"{error.Message} File='{strippedFileName} #{error.FileLocation.Line.Value}'";
        return $"{error.Message} File='{strippedFileName}'";
    }

    private static ErrorDescriptor GetDescriptor(XmlParseErrorKind xmlErrorErrorKind)
    {
        return xmlErrorErrorKind switch
        {
            XmlParseErrorKind.EmptyRoot => Diagnostics.Xml.EmptyRoot,
            XmlParseErrorKind.MissingFile => Diagnostics.Xml.MissingFile,
            XmlParseErrorKind.InvalidValue => Diagnostics.Xml.InvalidValue,
            XmlParseErrorKind.MalformedValue => Diagnostics.Xml.MalformedValue,
            XmlParseErrorKind.MissingAttribute => Diagnostics.Xml.MissingAttribute,
            XmlParseErrorKind.MissingReference => Diagnostics.Xml.MissingReference,
            XmlParseErrorKind.TooLongData => Diagnostics.Xml.ValueTooLong,
            XmlParseErrorKind.Unknown => Diagnostics.Xml.Generic,
            XmlParseErrorKind.DataBeforeHeader => Diagnostics.Xml.DataBeforeHeader,
            XmlParseErrorKind.MissingNode => Diagnostics.Xml.MissingNode,
            XmlParseErrorKind.UnknownNode => Diagnostics.Xml.UnknownNode,
            XmlParseErrorKind.TagHasElements => Diagnostics.Xml.TagHasElements,
            XmlParseErrorKind.UnexceptedElementName => Diagnostics.Xml.UnexpectedElementName,
            XmlParseErrorKind.EmptyNodeName => Diagnostics.Xml.EmptyNodeName,
            _ => throw new ArgumentOutOfRangeException(nameof(xmlErrorErrorKind), xmlErrorErrorKind, null)
        };
    }
}