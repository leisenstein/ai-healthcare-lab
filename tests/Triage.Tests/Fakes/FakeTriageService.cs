using Triage.Api.Contracts;
using Triage.Api.Interfaces;

namespace Triage.Tests.Fakes;

/// <summary>Stands in for the real service so endpoint tests only test the HTTP layer.</summary>
public sealed class FakeTriageService : ITriageService
{
    public List<TriageRequest> Requests { get; } = [];

    /// <summary>When set, TriageAsync throws this.</summary>
    public Exception? Throw { get; set; }

    public TriageResult Result { get; set; } = new(
        "P004", TriageCategory.Clinical, Urgency.Urgent, true,
        "Chest pain and arm numbness since this morning.",
        "Please call 911 or go to the nearest emergency room now.",
        new DateTime(2026, 12, 1, 11, 15, 0));

    public Task<TriageResult> TriageAsync(TriageRequest request, string? modelId = null, CancellationToken cancellationToken = default)
    {
        Requests.Add(request);
        return Throw is null ? Task.FromResult(Result) : Task.FromException<TriageResult>(Throw);
    }
}
