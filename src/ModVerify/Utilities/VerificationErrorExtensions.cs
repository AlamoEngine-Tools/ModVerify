using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting;

namespace AET.ModVerify.Utilities;

public static class VerificationErrorExtensions
{
    public static IEnumerable<VerificationError> ApplyBaseline(this IEnumerable<VerificationError> errors,
        VerificationBaseline baseline)
    {
        if (errors == null) 
            throw new ArgumentNullException(nameof(errors));
        if (baseline == null)
            throw new ArgumentNullException(nameof(baseline));
        return baseline.Apply(errors);
    }

    public static IEnumerable<VerificationError> ApplySuppressions(this IEnumerable<VerificationError> errors,
        SuppressionList suppressions)
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors));
        if (suppressions == null)
            throw new ArgumentNullException(nameof(suppressions));
        return suppressions.Apply(errors);
    }
}