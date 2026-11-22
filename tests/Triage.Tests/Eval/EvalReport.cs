using System.Globalization;
using System.Text;
using AiHealthLab.Core.Llm;

namespace Triage.Tests.Eval;

/// <summary>Turns scores and usage records into numbers and a Markdown table for the README.</summary>
public static class EvalReport
{
    public static ModelEvalSummary Summarize(
        string model, IReadOnlyList<EvalScore> scores, IReadOnlyList<ModelCallRecord> calls, IReadOnlyList<double> latenciesMs)
    {
        double Rate(Func<EvalScore, bool> correct) => scores.Count == 0 ? 0 : scores.Count(correct) / (double)scores.Count;

        return new ModelEvalSummary(
            Model: model,
            Cases: scores.Count,
            Errors: scores.Count(s => s.Failed),
            CategoryAccuracy: Rate(s => s.CategoryCorrect),
            UrgencyAccuracy: Rate(s => s.UrgencyCorrect),
            NeedsHumanAccuracy: Rate(s => s.NeedsHumanCorrect),
            MissedEscalations: scores.Count(s => s.MissedEscalation),
            AverageLatencyMs: latenciesMs.Count == 0 ? 0 : latenciesMs.Average(),
            InputTokens: calls.Sum(c => c.InputTokens),
            OutputTokens: calls.Sum(c => c.OutputTokens),
            TotalCostUsd: calls.Sum(c => c.CostUsd));
    }

    public static string ToMarkdown(IReadOnlyList<ModelRun> runs, DateTimeOffset ranAt)
    {
        var inv = CultureInfo.InvariantCulture;
        static string Pct(double v) => (v * 100).ToString("0", CultureInfo.InvariantCulture) + "%";

        var sb = new StringBuilder();
        sb.AppendLine($"## Triage eval ({ranAt.ToString("yyyy-MM-dd HH:mm", inv)})");
        sb.AppendLine();
        sb.AppendLine("| Model | Category | Urgency | needsHuman | Missed escalations | Errors | Avg latency | Tokens in / out | Cost (run) | Cost per 1,000 msgs |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (var s in runs.Select(r => r.Summary))
        {
            sb.AppendLine(string.Join(" | ",
                $"| {s.Model}",
                Pct(s.CategoryAccuracy),
                Pct(s.UrgencyAccuracy),
                Pct(s.NeedsHumanAccuracy),
                s.MissedEscalations.ToString(inv),
                s.Errors.ToString(inv),
                (s.AverageLatencyMs / 1000).ToString("0.0", inv) + " s",
                $"{s.InputTokens.ToString("N0", inv)} / {s.OutputTokens.ToString("N0", inv)}",
                "$" + s.TotalCostUsd.ToString("0.0000", inv),
                "$" + s.CostPer1000Messages.ToString("0.00", inv) + " |"));
        }

        foreach (var run in runs)
        {
            var misses = run.Scores.Where(x => !x.AllCorrect).ToList();
            sb.AppendLine();
            sb.AppendLine($"### {run.Summary.Model}: {misses.Count} of {run.Scores.Count} not fully correct");
            if (misses.Count == 0)
                continue;

            sb.AppendLine();
            sb.AppendLine("| Id | Expected | Got | Note |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var m in misses)
            {
                var e = m.Case.Expected;
                var got = m.Actual is { } a
                    ? $"{Lower(a.Category)} / {Lower(a.Urgency)} / human={Lower(a.NeedsHuman)}"
                    : $"ERROR: {m.Error}";
                var flag = m.MissedEscalation ? "**MISSED ESCALATION**" : "";
                sb.AppendLine($"| {m.Case.Id} | {Lower(e.Category)} / {Lower(e.Urgency)} / human={Lower(e.NeedsHuman)} | {got} | {flag} |");
            }
        }

        return sb.ToString();
    }

    private static string Lower(object value) => value.ToString()!.ToLowerInvariant();
}
