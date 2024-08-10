using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.Reporting;
using AET.ModVerify.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Verifiers;

internal sealed class XmlParseErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider) :
    InitializationErrorReporterBase<XmlError>(gameRepository, serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public override string Name => "XMLError";
    
    protected override void CreateError(XmlError error, out ErrorData errorData)
    {
        var id = GetIdFromError(error.ErrorKind);
        var severity = GetSeverityFromError(error.ErrorKind);

        var assets = new List<string>
        {
            _fileSystem.Path.GetGameStrippedPath(GameRepository.Path.AsSpan(), error.File.ToUpperInvariant().AsSpan()).ToString()
        };

        var xmlElement = error.Element;

        if (xmlElement is not null)
        {
            assets.Add(xmlElement.Name.LocalName);

            var parent = xmlElement.Parent;

            if (parent != null)
            {
                var parentName = parent.Attribute("Name");
                assets.Add(parentName != null ? $"parentName='{parentName.Value}'" : $"parentTag='{parent.Name.LocalName}'");
            }

        }

        errorData = new ErrorData(id, error.Message, assets, severity);
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
            _ => throw new ArgumentOutOfRangeException(nameof(xmlErrorErrorKind), xmlErrorErrorKind, null)
        };
    }
}