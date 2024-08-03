using System.Collections.Generic;
using System.Threading.Tasks;

namespace AET.ModVerify.Reporting;

public interface IVerificationReporter 
{
    public Task ReportAsync(IReadOnlyCollection<VerificationError> errors);
}