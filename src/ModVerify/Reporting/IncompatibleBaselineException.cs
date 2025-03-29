using System;

namespace AET.ModVerify.Reporting;

public sealed class IncompatibleBaselineException : Exception
{
    public override string Message => "The specified baseline is not compatible to this version of the application.";
}