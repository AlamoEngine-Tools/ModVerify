using System;

namespace AET.ModVerify.Reporting;

public sealed class InvalidBaselineException : Exception
{
    public InvalidBaselineException(string message) : base(message)
    {
    }

    public InvalidBaselineException(string? message, Exception? inner) : base(message, inner)
    {
    }
}