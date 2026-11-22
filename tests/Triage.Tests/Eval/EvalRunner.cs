using System.Diagnostics;
using AiHealthLab.Core.Llm;
using Triage.Api.Contracts;
using Triage.Api.Interfaces;

namespace Triage.Tests.Eval;

/// <summary>
/// Sends every case through the real ITriageService (the same code POST /triage uses),
/// one at a time, and scores it. Token counts and cost come from the cost-logging
/// records added during this run, not from estimates.
/// </summary>
public static class EvalRunner
{
    public static async Task<ModelRun> RunAsync(
        ITriageService triage, IUsageStore usage, IReadOnlyList<EvalCase> cases, string model,
        CancellationToken cancellationToken = default)
    {
        var recordsBefore = usage.All().Count;
        var scores = new List<EvalScore>();
        var latencies = new List<double>();

        foreach (var c in cases)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                var result = await triage.TriageAsync(new TriageRequest(c.PatientId, c.Message), model, cancellationToken);
                scores.Add(new EvalScore(c, result, null));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                scores.Add(new EvalScore(c, null, $"{ex.GetType().Name}: {ex.Message}"));
            }
            latencies.Add(timer.Elapsed.TotalMilliseconds);
        }

        var calls = usage.All().Skip(recordsBefore).ToList();
        return new ModelRun(EvalReport.Summarize(model, scores, calls, latencies), scores);
    }
}
