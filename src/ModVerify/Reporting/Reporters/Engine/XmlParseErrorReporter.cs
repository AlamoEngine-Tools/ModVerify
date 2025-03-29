using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.Utilities;
using AET.ModVerify.Verifiers;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Reporting.Reporters.Engine;

internal sealed class XmlParseErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider) :
    EngineErrorReporterBase<XmlError>(gameRepository, serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public override string Name => "XMLError";
    
    protected override ErrorData CreateError(XmlError error)
    {
        var id = GetIdFromError(error.ErrorKind);
        var severity = GetSeverityFromError(error.ErrorKind);

        var strippedFileName = _fileSystem.Path
            .GetGameStrippedPath(GameRepository.Path.AsSpan(), error.FileLocation.XmlFile.ToUpperInvariant().AsSpan()).ToString();

        var asset = strippedFileName;
        
        var context = new List<string>
        {
            strippedFileName
        };

        var xmlElement = error.Element;

        if (xmlElement is not null)
        {
            var localName = xmlElement.Name.LocalName;
            context.Add(localName);
            
            asset = localName;

            var parent = xmlElement.Parent;

            if (parent != null)
            {
                var parentName = parent.Attribute("Name");
                context.Add(parentName != null ? $"parentName='{parentName.Value}'" : $"parentTag='{parent.Name.LocalName}'");
            }
        }

        var errorMessage = CreateErrorMessage(error, strippedFileName);
        return new ErrorData(id, errorMessage, context, asset, severity);
    }

    private static string CreateErrorMessage(XmlError error, string strippedFileName)
    {
        if (error.FileLocation.Line.HasValue)
            return $"{error.Message} File='{strippedFileName} #{error.FileLocation.Line.Value}'";
        return $"{error.Message} File='{strippedFileName}'";
    }

    private static VerificationSeverity GetSeverityFromError(XmlParseErrorKind xmlErrorErrorKind)
    {
        return xmlErrorErrorKind switch
        {
            XmlParseErrorKind.EmptyRoot => VerificationSeverity.Critical,
            XmlParseErrorKind.MissingFile => VerificationSeverity.Error,
            XmlParseErrorKind.InvalidValue => VerificationSeverity.Information,
            XmlParseErrorKind.MalformedValue => VerificationSeverity.Warning,
            XmlParseErrorKind.MissingAttribute => VerificationSeverity.Error,
            XmlParseErrorKind.MissingReference => VerificationSeverity.Error,
            XmlParseErrorKind.TooLongData => VerificationSeverity.Warning,
            XmlParseErrorKind.DataBeforeHeader => VerificationSeverity.Information,
            XmlParseErrorKind.MissingNode => VerificationSeverity.Critical,
            XmlParseErrorKind.UnknownNode => VerificationSeverity.Information,
            _ => VerificationSeverity.Warning
        };
    }

    private static string GetIdFromError(XmlParseErrorKind xmlErrorErrorKind)
    {
        return xmlErrorErrorKind switch
        {
            XmlParseErrorKind.EmptyRoot => VerifierErrorCodes.EmptyXmlRoot,
            XmlParseErrorKind.MissingFile => VerifierErrorCodes.MissingXmlFile,
            XmlParseErrorKind.InvalidValue => VerifierErrorCodes.InvalidXmlValue,
            XmlParseErrorKind.MalformedValue => VerifierErrorCodes.MalformedXmlValue,
            XmlParseErrorKind.MissingAttribute => VerifierErrorCodes.MissingXmlAttribute,
            XmlParseErrorKind.MissingReference => VerifierErrorCodes.MissingXmlReference,
            XmlParseErrorKind.TooLongData => VerifierErrorCodes.XmlValueTooLong,
            XmlParseErrorKind.Unknown => VerifierErrorCodes.GenericXmlError,
            XmlParseErrorKind.DataBeforeHeader => VerifierErrorCodes.XmlDataBeforeHeader,
            XmlParseErrorKind.MissingNode => VerifierErrorCodes.XmlMissingNode,
            XmlParseErrorKind.UnknownNode => VerifierErrorCodes.XmlUnsupportedTag,
            _ => throw new ArgumentOutOfRangeException(nameof(xmlErrorErrorKind), xmlErrorErrorKind, null)
        };
    }
}