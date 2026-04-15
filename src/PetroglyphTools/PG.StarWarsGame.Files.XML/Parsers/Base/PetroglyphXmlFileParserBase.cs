using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Utilities;
using PG.StarWarsGame.Files.XML.ErrorHandling;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParserBase(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter) 
    : PetroglyphXmlParserBase(errorReporter)
{
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    protected virtual bool LoadLineInfo => true;

    protected XElement GetRootElement(Stream xmlStream, out string fileName)
    {
        fileName = GetStrippedFilePath(xmlStream.GetFilePath());

        if (string.IsNullOrEmpty(fileName))
            throw new InvalidOperationException("Unable to parse XML from unnamed stream. Either parse from a file or MEG stream.");

        SkipCharactersUntilXmlHeader(fileName, xmlStream);

        var asciiStreamReader = new StreamReader(xmlStream, XmlFileConstants.XmlEncoding, false, 1024, leaveOpen: true);
        using var xmlReader = XmlReader.Create(asciiStreamReader, new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            CloseInput = true
        }, fileName);

        var options = LoadOptions.SetBaseUri;
        if (LoadLineInfo)
            options |= LoadOptions.SetLineInfo;

        var doc = XDocument.Load(xmlReader, options);

        var root = doc.Root;

        if (root is null)
            throw new XmlException("No root node found.");

        if (!root.HasElements)
        {
            ErrorReporter?.Report(new XmlError(this, root)
            {
                ErrorKind = XmlParseErrorKind.EmptyRoot,
                Message = "XML file has an empty root node.",
            });
        }

        return root;
    }
    
    private string GetStrippedFilePath(string filePath)
    {
        if (!FileSystem.Path.IsPathFullyQualified(filePath))
            return filePath;
        
        var pathPartIndex = filePath.LastIndexOf(GetXmlDataFolder(), StringComparison.OrdinalIgnoreCase);

        if (pathPartIndex == -1)
            return filePath;

        return filePath[pathPartIndex..];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetXmlDataFolder()
        {
            // Required because we don't have access to the PGFileSystem here.
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"DATA\XML\" : "DATA/XML/";
        }
    }


    private void SkipCharactersUntilXmlHeader(string fileName, Stream stream)
    {
        using var r = new StreamReader(stream, XmlFileConstants.XmlEncoding, false, 10, true);
        var count = 0;

        // It might be possible, that a XML file starts with leading spaces or even a encoding BOM. 
        // The engine skips everything until the first '<' character, so we have to do the same to avoid parsing errors.

        while ((char)r.Read() != '<') 
            count++;

        if (count != 0)
        {
            ErrorReporter?.Report(new XmlError(this, locationInfo: new XmlLocationInfo(fileName, 0))
            {
                ErrorKind = XmlParseErrorKind.DataBeforeHeader,
                Message = "XML header is not the first entry of the XML file.",
            });
        }

        stream.Position = count;
    }
}