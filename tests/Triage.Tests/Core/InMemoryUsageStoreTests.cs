using AiHealthLab.Core.Llm;

namespace Triage.Tests.Core;

public class InMemoryUsageStoreTests
{
    private static ModelCallRecord Call(string model, long input, long output, int ms, decimal cost) =>
        new(DateTimeOffset.UtcNow, model, input, output, TimeSpan.FromMilliseconds(ms), cost);

    [Fact]
    public void Empty_store_summarizes_to_zero()
    {
        var summary = new InMemoryUsageStore().Summarize();

        Assert.Equal(0, summary.Calls);
        Assert.Equal(0m, summary.CostUsd);
        Assert.Equal(0, summary.AverageLatencyMs);
        Assert.Empty(summary.ByModel);
    }

    [Fact]
    public void Summary_totals_everything_and_groups_by_model()
    {
        var store = new InMemoryUsageStore();
        store.Add(Call("claude-sonnet-5-5", 1000, 200, 800, 0.004m));
        store.Add(Call("claude-sonnet-5-5", 3000, 400, 1200, 0.010m));
        store.Add(Call("claude-haiku-5-5", 1000, 200, 300, 0.0002m));

        var summary = store.Summarize();

        Assert.Equal(3, summary.Calls);
        Assert.Equal(5000, summary.InputTokens);
        Assert.Equal(800, summary.OutputTokens);
        Assert.Equal(0.0142m, summary.CostUsd);
        Assert.Equal(766.67, summary.AverageLatencyMs, precision: 2);

        var sonnet = Assert.Single(summary.ByModel, m => m.Model == "claude-sonnet-5-5");
        Assert.Equal(2, sonnet.Calls);
        Assert.Equal(0.014m, sonnet.CostUsd);
        Assert.Equal(1000, sonnet.AverageLatencyMs);
    }

    [Fact]
    public void Concurrent_adds_are_all_kept()
    {
        var store = new InMemoryUsageStore();

        Parallel.For(0, 1000, _ => store.Add(Call("m", 1, 1, 1, 0m)));

        Assert.Equal(1000, store.All().Count);
    }
}
