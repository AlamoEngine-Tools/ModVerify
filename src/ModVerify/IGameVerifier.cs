using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;

namespace AET.ModVerify;

public interface IGameVerifier
{
    IGameVerifier? Parent { get; }

    string Name { get; }

    string FriendlyName { get; }

    IReadOnlyCollection<VerificationError> VerifyErrors { get; }

    void Verify(CancellationToken token);
}