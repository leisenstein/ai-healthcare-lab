namespace AiHealthLab.Core.Llm;

/// <summary>Thread-safe, process-lifetime store. Resets when the API restarts.</summary>
public sealed class InMemoryUsageStore : IUsageStore
{
    private readonly List<ModelCallRecord> _records = [];
    private readonly Lock _lock = new();

    public void Add(ModelCallRecord record)
    {
        lock (_lock) _records.Add(record);
    }

    public IReadOnlyList<ModelCallRecord> All()
    {
        lock (_lock) return [.. _records];
    }

    public UsageSummary Summarize()
    {
        var records = All();

        var byModel = records
            .GroupBy(r => r.Model, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ModelUsage(
                g.Key,
                g.Count(),
                g.Sum(r => r.InputTokens),
                g.Sum(r => r.OutputTokens),
                g.Sum(r => r.CostUsd),
                g.Average(r => r.Latency.TotalMilliseconds)))
            .OrderBy(m => m.Model)
            .ToList();

        return new UsageSummary(
            records.Count,
            records.Sum(r => r.InputTokens),
            records.Sum(r => r.OutputTokens),
            records.Sum(r => r.CostUsd),
            records.Count == 0 ? 0 : records.Average(r => r.Latency.TotalMilliseconds),
            byModel);
    }
}
