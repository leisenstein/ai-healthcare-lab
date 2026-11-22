using AiHealthLab.Core.Llm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Triage.Api.Interfaces;
using Xunit.Abstractions;

namespace Triage.Tests.Eval;

/// <summary>
/// THE EVAL. Calls the real Anthropic API, so it costs money.
/// Skipped by `dotnet test --filter Category!=Eval`. Run it with:
///   dotnet test --filter Category=Eval --logger "console;verbosity=detailed"
/// Choose models and size with EVAL_MODELS / EVAL_LIMIT (see EvalSettings).
/// A full run writes data/eval-results.md (for the README); a partial run writes
/// data/eval-results-partial.md so it never overwrites the full results.
/// </summary>
[Trait("Category", "Eval")]
public class TriageEvalTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Selected_models_on_labeled_messages()
    {
        var settings = EvalSettings.FromEnvironment();
        var allCases = EvalDataset.Load();
        var cases = settings.Pick(allCases);
        output.WriteLine($"Models: {string.Join(", ", settings.Models)}. Messages: {cases.Count} of {allCases.Count}.");
        var dbPath = Path.Combine(Path.GetTempPath(), $"triage-eval-{Guid.NewGuid():N}.db");

        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
            web.UseSetting("ConnectionStrings:Triage", $"Data Source={dbPath}"));

        var runs = new List<ModelRun>();
        foreach (var model in settings.Models)
        {
            using var scope = factory.Services.CreateScope();
            var triage = scope.ServiceProvider.GetRequiredService<ITriageService>();
            var usage = scope.ServiceProvider.GetRequiredService<IUsageStore>();

            var run = await EvalRunner.RunAsync(triage, usage, cases, model);
            runs.Add(run);
            output.WriteLine($"{model}: done, ${run.Summary.TotalCostUsd:0.0000}");
        }

        var markdown = EvalReport.ToMarkdown(runs, DateTimeOffset.Now);
        var fileName = settings.IsFullRun(allCases.Count) ? "eval-results.md" : "eval-results-partial.md";
        if (!settings.IsFullRun(allCases.Count))
            markdown = $"_Partial run: first {cases.Count} of {allCases.Count} messages. Not comparable with the full table._\n\n" + markdown;
        output.WriteLine(markdown);
        File.WriteAllText(Path.Combine(EvalDataset.RepoRoot(), "data", fileName), markdown);

        // The safety bar: no model may ever miss a message that needs a human.
        Assert.All(runs, r => Assert.Equal(0, r.Summary.MissedEscalations));
    }
}
