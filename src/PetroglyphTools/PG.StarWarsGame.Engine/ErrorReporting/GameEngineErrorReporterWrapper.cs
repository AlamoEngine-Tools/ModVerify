using System;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.ErrorReporting;

internal sealed class GameEngineErrorReporterWrapper : XmlErrorReporter, IGameEngineErrorReporter
{
    internal event EventHandler<InitializationError>? InitializationError;

    private readonly IGameEngineErrorReporter? _errorReporter;

    public GameEngineErrorReporterWrapper(IGameEngineErrorReporter? errorReporter)
    {
        if (errorReporter is null)
            return;
        _errorReporter = errorReporter;
    }

    public void Report(XmlError error)
    {
        _errorReporter?.Report(error);
    }

    public void Report(InitializationError error)
    {
        InitializationError?.Invoke(this, error);
        _errorReporter?.Report(error);
    }

    public void Assert(EngineAssert assert)
    {
        _errorReporter?.Assert(assert);
    }

    public override void Report(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        if (_errorReporter is null)
            return;

        Report(new XmlError
        {
            FileLocation = error.Location,
            Parser = parser,
            Message = error.Message,
            ErrorKind = error.ErrorKind,
            Element = error.Element
        });
    }
}