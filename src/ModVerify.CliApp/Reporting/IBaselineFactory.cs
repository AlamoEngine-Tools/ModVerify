using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AET.ModVerify.App.Reporting;

internal interface IBaselineFactory
{
    bool TryFindBaselineInDirectory(
        string directory,
        Predicate<VerificationBaseline> baselineSelector,
        [NotNullWhen(true)] out VerificationBaseline? baseline, 
        [NotNullWhen(true)] out string? path);

    VerificationBaseline ParseBaseline(string filePath);

    Task WriteBaselineAsync(VerificationBaseline baseline, string filePath);

    VerificationBaseline CreateBaseline(
        VerificationTarget target, 
        AppBaselineSettings settings, 
        IEnumerable<VerificationError> errors);
}