namespace AiHealthLab.Core.Llm;

/// <summary>Totals for GET /stats.</summary>
public sealed record UsageSummary(
    int Calls,
    long InputTokens,
    long OutputTokens,
    decimal CostUsd,
    double AverageLatencyMs,
    IReadOnlyList<ModelUsage> ByModel);

public sealed record ModelUsage(
    string Model,
    int Calls,
    long InputTokens,
    long OutputTokens,
    decimal CostUsd,
    double AverageLatencyMs);
