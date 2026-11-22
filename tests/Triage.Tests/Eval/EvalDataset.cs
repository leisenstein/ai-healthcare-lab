using System.Text.Json;
using Triage.Api.Contracts;

namespace Triage.Tests.Eval;

/// <summary>Loads data/messages.json from the repo root.</summary>
public static class EvalDataset
{
    private sealed record File(List<EvalCase> Messages);

    public static IReadOnlyList<EvalCase> Load(string? path = null)
    {
        path ??= Path.Combine(RepoRoot(), "data", "messages.json");
        var file = JsonSerializer.Deserialize<File>(System.IO.File.ReadAllText(path), TriageJson.Options)
            ?? throw new InvalidOperationException($"{path} is empty.");
        return file.Messages;
    }

    /// <summary>Walks up from the test's bin folder until it finds data/messages.json.</summary>
    public static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (System.IO.File.Exists(Path.Combine(dir.FullName, "data", "messages.json")))
                return dir.FullName;

        throw new FileNotFoundException("Could not find data/messages.json above " + AppContext.BaseDirectory);
    }
}
