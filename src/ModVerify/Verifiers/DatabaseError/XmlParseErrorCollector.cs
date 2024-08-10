using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Verifiers;

public sealed class XmlParseErrorCollector(
    IEnumerable<XmlParseErrorEventArgs> xmlErrors,
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) :
    GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public override string FriendlyName => "XML Parsing Errors";

    protected override void RunVerification(CancellationToken token)
    {
        foreach (var xmlError in xmlErrors)
            AddError(ConvertXmlToVerificationError(xmlError));
    }

    private VerificationError ConvertXmlToVerificationError(XmlParseErrorEventArgs xmlError)
    {
        var id = GetIdFromError(xmlError.ErrorKind);
        var severity = GetSeverityFromError(xmlError.ErrorKind);

        var assets = new List<string>
        {
            GetGameStrippedPath(xmlError.File.ToUpperInvariant())
        };

        var xmlElement = xmlError.Element;

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

        return VerificationError.Create(this, id, xmlError.Message, severity, assets);

    }

    private VerificationSeverity GetSeverityFromError(XmlParseErrorKind xmlErrorErrorKind)
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

    private string GetIdFromError(XmlParseErrorKind xmlErrorErrorKind)
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