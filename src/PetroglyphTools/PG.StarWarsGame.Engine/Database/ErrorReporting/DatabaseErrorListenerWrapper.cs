using System;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

internal class DatabaseErrorListenerWrapper : DisposableObject, IDatabaseErrorListener, IXmlParserErrorListener
{
    internal event EventHandler<InitializationError>? InitializationError; 

    private readonly IDatabaseErrorListener? _errorListener;
    private IPrimitiveXmlErrorParserProvider? _primitiveXmlParserErrorProvider;

    public DatabaseErrorListenerWrapper(IDatabaseErrorListener? errorListener, IServiceProvider serviceProvider)
    {
        _errorListener = errorListener;
        if (_errorListener is null)
            return;
        _primitiveXmlParserErrorProvider = serviceProvider.GetRequiredService<IPrimitiveXmlErrorParserProvider>();
        _primitiveXmlParserErrorProvider.XmlParseError += ((IXmlParserErrorListener)this).OnXmlParseError;
    }

    public void OnXmlError(XmlError error)
    {
        _errorListener?.OnXmlError(error);
    }

    public void OnInitializationError(InitializationError error)
    {
        InitializationError?.Invoke(this, error);
        if (_errorListener is null)
            return;
        _errorListener.OnInitializationError(error);
    }

    protected override void DisposeResources()
    {
        base.DisposeResources();
        if (_primitiveXmlParserErrorProvider is null)
            return;
        _primitiveXmlParserErrorProvider.XmlParseError -= ((IXmlParserErrorListener)this).OnXmlParseError;
        _primitiveXmlParserErrorProvider = null!;
    }

    void IXmlParserErrorListener.OnXmlParseError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        if (_errorListener is null)
            return;

        OnXmlError(new XmlError
        {
            FileLocation = error.Location,
            Parser = parser.ToString(),
            Message = error.Message,
            ErrorKind = error.ErrorKind,
            Element = error.Element
        });
    }
}