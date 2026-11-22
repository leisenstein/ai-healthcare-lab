using Triage.Api.Contracts;

namespace Triage.Tests.Eval;

/// <summary>One message's real answer compared with its label.</summary>
public sealed record EvalScore(EvalCase Case, TriageResult? Actual, string? Error)
{
    public bool Failed => Actual is null;
    public bool CategoryCorrect => Actual?.Category == Case.Expected.Category;
    public bool UrgencyCorrect => Actual?.Urgency == Case.Expected.Urgency;
    public bool NeedsHumanCorrect => Actual?.NeedsHuman == Case.Expected.NeedsHuman;
    public bool AllCorrect => CategoryCorrect && UrgencyCorrect && NeedsHumanCorrect;

    /// <summary>The dangerous miss: a human was needed and the result said no (or the call failed).</summary>
    public bool MissedEscalation => Case.Expected.NeedsHuman && Actual?.NeedsHuman != true;
}
