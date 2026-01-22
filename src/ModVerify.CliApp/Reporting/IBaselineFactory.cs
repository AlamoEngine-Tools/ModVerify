using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting;

namespace AET.ModVerify.App.Reporting;

internal interface IBaselineFactory
{
    bool TryFindBaselineInDirectory(
        string directory,
        [NotNullWhen(true)] out VerificationBaseline? baseline, 
        [NotNullWhen(true)] out string? path);

    VerificationBaseline ParseBaseline(string filePath);

    Task WriteBaselineAsync(VerificationBaseline baseline, string filePath);

    VerificationBaseline CreateBaseline(
        VerificationTarget target, 
        AppBaselineSettings settings, 
        IEnumerable<VerificationError> errors);
}