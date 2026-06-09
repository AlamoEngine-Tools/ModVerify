using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting;

public sealed class VerificationErrors
{
    public static readonly VerificationErrors Empty = new(
        [],
        ReadOnlyValueListDictionary<string, VerificationError>.Empty,
        ReadOnlyValueListDictionary<string, VerificationError>.Empty);

    public IReadOnlyList<VerificationError> NewErrors { get; }

    public IReadOnlyValueListDictionary<string, VerificationError> ExistingErrors { get; }

    public IReadOnlyValueListDictionary<string, VerificationError> ResolvedErrors { get; }

    public VerificationErrors(
        IReadOnlyList<VerificationError> newErrors,
        IReadOnlyValueListDictionary<string, VerificationError> existingErrors,
        IReadOnlyValueListDictionary<string, VerificationError> resolvedErrors)
    {
        NewErrors = newErrors ?? throw new ArgumentNullException(nameof(newErrors));
        ExistingErrors = existingErrors ?? throw new ArgumentNullException(nameof(existingErrors));
        ResolvedErrors = resolvedErrors ?? throw new ArgumentNullException(nameof(resolvedErrors));
    }
}
