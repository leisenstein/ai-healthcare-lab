using AiHealthLab.Core.Llm;
using Triage.Api.Contracts;
using Triage.Tests.Fakes;

namespace Triage.Tests.Eval;

/// <summary>The runner with a fake service: free, and proves the loop and the cost window work.</summary>
public class EvalRunnerTests
{
    private static readonly EvalCase[] Cases =
    [
        new("m1", "P004", "chest pain", new ExpectedLabel(TriageCategory.Clinical, Urgency.Urgent, true)),
        new("m2", "P001", "refill please", new ExpectedLabel(TriageCategory.Refill, Urgency.Routine, false)),
    ];

    [Fact]
    public async Task Sends_every_case_with_the_model_and_scores_it()
    {
        var triage = new FakeTriageService();   // always answers clinical / urgent / needsHuman
        var usage = new InMemoryUsageStore();

        var run = await EvalRunner.RunAsync(triage, usage, Cases, "claude-haiku-5-5");

        Assert.Equal(new[] { "chest pain", "refill please" }, triage.Requests.Select(r => r.Message));
        Assert.Equal(2, run.Summary.Cases);
        Assert.Equal(0.5, run.Summary.CategoryAccuracy);
        Assert.Equal(0, run.Summary.MissedEscalations);
    }

    [Fact]
    public async Task Only_counts_cost_from_calls_made_during_this_run()
    {
        var usage = new InMemoryUsageStore();
        usage.Add(new ModelCallRecord(DateTimeOffset.UtcNow, "old", 999, 999, TimeSpan.Zero, 5m));   // an earlier run

        var run = await EvalRunner.RunAsync(new FakeTriageService(), usage, Cases, "m");

        Assert.Equal(0m, run.Summary.TotalCostUsd);   // the fake makes no model calls
    }

    [Fact]
    public async Task A_failing_message_is_recorded_as_an_error_not_a_crash()
    {
        var triage = new FakeTriageService { Throw = new InvalidOperationException("boom") };

        var run = await EvalRunner.RunAsync(triage, new InMemoryUsageStore(), Cases, "m");

        Assert.Equal(2, run.Summary.Errors);
        Assert.Equal(1, run.Summary.MissedEscalations);   // m1 needed a human and got nothing
        Assert.Contains("boom", run.Scores[0].Error);
    }
}
