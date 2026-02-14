using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;

namespace AET.ModVerify.Utilities;

public static class VerificationErrorExtensions
{
    extension(IEnumerable<VerificationError> errors)
    {
        public IEnumerable<VerificationError> ApplyBaseline(VerificationBaseline baseline)
        {
            if (errors == null) 
                throw new ArgumentNullException(nameof(errors));
            if (baseline == null)
                throw new ArgumentNullException(nameof(baseline));
            return baseline.Apply(errors);
        }

        public IEnumerable<VerificationError> ApplySuppressions(SuppressionList suppressions)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));
            if (suppressions == null)
                throw new ArgumentNullException(nameof(suppressions));
            return suppressions.Apply(errors);
        }
    }
}