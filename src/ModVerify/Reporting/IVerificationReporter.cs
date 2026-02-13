using System.Threading.Tasks;

namespace AET.ModVerify.Reporting;

public interface IVerificationReporter 
{
    public Task ReportAsync(VerificationResult verificationResult);
}