using AiHealthLab.Core.Llm;
using Triage.Api.Contracts;

namespace Triage.Tests.Eval;

public class EvalReportTests
{
    private static EvalCase Case(string id, TriageCategory cat, Urgency urg, bool human) =>
        new(id, "P001", "msg", new ExpectedLabel(cat, urg, human));

    private static TriageResult Got(TriageCategory cat, Urgency urg, bool human) =>
        new("P001", cat, urg, human, "s", "d", null);

    private static readonly EvalScore Right = new(Case("m1", TriageCategory.Refill, Urgency.Soon, false), Got(TriageCategory.Refill, Urgency.Soon, false), null);
    private static readonly EvalScore WrongUrgency = new(Case("m2", TriageCategory.Billing, Urgency.Routine, false), Got(TriageCategory.Billing, Urgency.Soon, false), null);
    private static readonly EvalScore Missed = new(Case("m3", TriageCategory.Clinical, Urgency.Urgent, true), Got(TriageCategory.Clinical, Urgency.Urgent, false), null);
    private static readonly EvalScore Errored = new(Case("m4", TriageCategory.Clinical, Urgency.Soon, true), null, "TriageParseException: bad");

    [Fact]
    public void Score_flags_a_missed_escalation_and_counts_errors_as_misses()
    {
        Assert.True(Right.AllCorrect);
        Assert.False(WrongUrgency.AllCorrect);
        Assert.True(WrongUrgency.CategoryCorrect);
        Assert.True(Missed.MissedEscalation);
        Assert.True(Errored.MissedEscalation);   // a crash on a clinical message is still a miss
        Assert.False(Right.MissedEscalation);
    }

    [Fact]
    public void Summary_computes_accuracy_cost_and_cost_per_1000()
    {
        var calls = new[]
        {
            new ModelCallRecord(DateTimeOffset.UtcNow, "m", 1000, 100, TimeSpan.FromSeconds(1), 0.003m),
            new ModelCallRecord(DateTimeOffset.UtcNow, "m", 1000, 100, TimeSpan.FromSeconds(1), 0.003m),
        };

        var s = EvalReport.Summarize("m", [Right, WrongUrgency, Missed, Errored], calls, [1000, 2000, 3000, 4000]);

        Assert.Equal(4, s.Cases);
        Assert.Equal(1, s.Errors);
        Assert.Equal(0.75, s.CategoryAccuracy);
        Assert.Equal(0.5, s.UrgencyAccuracy);
        Assert.Equal(2, s.MissedEscalations);
        Assert.Equal(2500, s.AverageLatencyMs);
        Assert.Equal(2000, s.InputTokens);
        Assert.Equal(0.006m, s.TotalCostUsd);
        Assert.Equal(1.5m, s.CostPer1000Messages);   // $0.006 / 4 messages * 1000
    }

    [Fact]
    public void Markdown_has_the_table_and_lists_only_the_misses()
    {
        var summary = EvalReport.Summarize("claude-haiku-5-5", [Right, Missed], [], [1000, 1000]);

        var md = EvalReport.ToMarkdown([new ModelRun(summary, [Right, Missed])], new DateTimeOffset(2026, 11, 22, 9, 0, 0, TimeSpan.Zero));

        Assert.Contains("| Model | Category | Urgency | needsHuman | Missed escalations |", md);
        Assert.Contains("| claude-haiku-5-5 | 100% | 100% | 50% | 1 |", md);
        Assert.Contains("1 of 2 not fully correct", md);
        Assert.Contains("| m3 |", md);
        Assert.Contains("MISSED ESCALATION", md);
        Assert.DoesNotContain("| m1 |", md);
    }
}
