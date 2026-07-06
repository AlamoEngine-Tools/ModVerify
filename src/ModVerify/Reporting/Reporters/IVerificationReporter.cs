using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Defines a reporter that writes a verification result to an output sink.</summary>
public interface IVerificationReporter
{
    /// <summary>Asynchronously reports the specified verification result.</summary>
    /// <param name="verificationResult">The verification result to report.</param>
    /// <returns>A task that represents the asynchronous report operation.</returns>
    public Task ReportAsync(VerificationResult verificationResult);
}