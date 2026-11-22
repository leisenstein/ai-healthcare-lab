using System.Diagnostics.CodeAnalysis;
using Triage.Api.Contracts;

namespace Triage.Api.Services;

/// <summary>Either a valid classification or the reason it was rejected.</summary>
public sealed record ParseOutcome(TriageClassification? Value, string? Error)
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Value is not null;

    public static ParseOutcome Ok(TriageClassification value) => new(value, null);
    public static ParseOutcome Fail(string error) => new(null, error);
}
