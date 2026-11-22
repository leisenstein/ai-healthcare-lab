namespace Triage.Tests.Eval;

/// <summary>Totals for one model over the whole dataset.</summary>
public sealed record ModelEvalSummary(
    string Model,
    int Cases,
    int Errors,
    double CategoryAccuracy,
    double UrgencyAccuracy,
    double NeedsHumanAccuracy,
    int MissedEscalations,
    double AverageLatencyMs,
    long InputTokens,
    long OutputTokens,
    decimal TotalCostUsd)
{
    public decimal CostPer1000Messages => Cases == 0 ? 0 : TotalCostUsd / Cases * 1000;
}

/// <summary>Summary plus every per-message score, so the report can list the misses.</summary>
public sealed record ModelRun(ModelEvalSummary Summary, IReadOnlyList<EvalScore> Scores);
