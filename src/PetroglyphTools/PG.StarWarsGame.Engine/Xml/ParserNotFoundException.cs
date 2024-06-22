using System;

namespace PG.StarWarsGame.Engine.Xml;

public sealed class ParserNotFoundException : Exception
{
    public override string Message { get; }

    public ParserNotFoundException(Type type)
    {
        Message = $"The parser for the type {type} was not found.";
    }

    public ParserNotFoundException(string tag)
    {
        Message = $"The parser for the tag {tag} was not found.";
    }
}