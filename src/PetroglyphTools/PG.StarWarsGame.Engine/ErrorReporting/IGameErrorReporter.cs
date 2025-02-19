﻿namespace PG.StarWarsGame.Engine.ErrorReporting;

public interface IGameErrorReporter
{
    void Report(XmlError error);

    void Report(InitializationError error);

    void Assert(EngineAssert assert);
}