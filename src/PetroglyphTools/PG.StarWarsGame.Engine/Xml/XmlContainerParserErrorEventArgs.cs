using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace PG.StarWarsGame.Engine.Xml;

internal class XmlContainerParserErrorEventArgs
{
    private bool _continue = true;

    public bool Continue
    {
        get => _continue;
        // Once this is set to false, there is no way back.
        set => _continue &= value;
    }

    public bool IsContainer { get; }

    public string File { get; }

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsError => Exception is not null;

    public bool IsFileNotFound => !IsError;

    public XmlException? Exception { get; }

    public XmlContainerParserErrorEventArgs(XmlException exception, string file, bool container = false)
    {
        Exception = exception;
        File = file;
        IsContainer = container;
    }

    public XmlContainerParserErrorEventArgs(string file, bool container = false)
    {
        File = file;
        IsContainer = container;
    }
}