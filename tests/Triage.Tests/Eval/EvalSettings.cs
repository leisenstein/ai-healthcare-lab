namespace Triage.Tests.Eval;

/// <summary>
/// Which models and how many messages the paid eval runs, read from environment variables:
///   EVAL_MODELS  comma list of "sonnet", "haiku", "opus" or full model ids (default: sonnet,haiku)
///   EVAL_LIMIT   only run the first N messages (default: all)
/// Defaults give the full Sonnet + Haiku run.
/// </summary>
public sealed record EvalSettings(IReadOnlyList<string> Models, int? Limit)
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sonnet"] = "claude-sonnet-5-5",
        ["haiku"] = "claude-haiku-5-5",
        ["opus"] = "claude-opus-5-5",
    };

    public static readonly IReadOnlyList<string> DefaultModels = ["claude-sonnet-5-5", "claude-haiku-5-5"];

    public static EvalSettings FromEnvironment() => From(Environment.GetEnvironmentVariable);

    /// <summary>Takes a lookup function so tests can pass fake variables.</summary>
    public static EvalSettings From(Func<string, string?> getVariable)
    {
        var models = (getVariable("EVAL_MODELS") ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(m => Aliases.GetValueOrDefault(m, m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        int? limit = null;
        var rawLimit = getVariable("EVAL_LIMIT");
        if (!string.IsNullOrWhiteSpace(rawLimit))
        {
            if (!int.TryParse(rawLimit, out var n) || n < 1)
                throw new ArgumentException($"EVAL_LIMIT must be a whole number of 1 or more, got '{rawLimit}'.");
            limit = n;
        }

        return new EvalSettings(models.Count > 0 ? models : DefaultModels, limit);
    }

    /// <summary>The first <see cref="Limit"/> cases, or all of them.</summary>
    public IReadOnlyList<EvalCase> Pick(IReadOnlyList<EvalCase> cases) =>
        Limit is { } n ? cases.Take(n).ToList() : cases;

    public bool IsFullRun(int totalCases) =>
        Limit is null || Limit >= totalCases;
}
