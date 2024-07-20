using System.Collections.Generic;
using AET.ModVerify.Reporting;

namespace AET.ModVerify;

public interface IGameVerifier
{
    string Name { get; }

    string FriendlyName { get; }

    IReadOnlyCollection<VerificationError> VerifyErrors { get; }
}