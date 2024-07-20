using System.Collections.Generic;

namespace AET.ModVerify.Reporting;

public interface IVerificationReporter 
{
    public void Report(IReadOnlyCollection<VerificationError> errors);
}