using System;

namespace AET.ModVerify.Reporting.Baseline;

public sealed class InvalidBaselineException : Exception
{
    internal InvalidBaselineException(string message) : base(message)
    {
    }

    internal InvalidBaselineException(string? message, Exception? inner) : base(message, inner)
    {
    }
}