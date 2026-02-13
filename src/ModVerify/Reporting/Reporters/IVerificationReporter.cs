using System.Threading.Tasks;

namespace AET.ModVerify.Reporting.Reporters;

public interface IVerificationReporter 
{
    public Task ReportAsync(VerificationResult verificationResult);
}