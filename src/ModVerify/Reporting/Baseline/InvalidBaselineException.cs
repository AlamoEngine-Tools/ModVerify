using System;

namespace AET.ModVerify.Reporting.Baseline;

/// <summary>The exception that is thrown when a verification baseline cannot be parsed or is otherwise invalid.</summary>
public sealed class InvalidBaselineException : Exception
{
    internal InvalidBaselineException(string message) : base(message)
    {
    }

    internal InvalidBaselineException(string? message, Exception? inner) : base(message, inner)
    {
    }
}