namespace AiHealthLab.Core.Llm;

/// <summary>One model API call. Holds numbers only, never prompt or reply text.</summary>
public sealed record ModelCallRecord(
    DateTimeOffset At,
    string Model,
    long InputTokens,
    long OutputTokens,
    TimeSpan Latency,
    decimal CostUsd);
