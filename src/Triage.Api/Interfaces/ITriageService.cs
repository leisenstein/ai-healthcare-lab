using Triage.Api.Contracts;

namespace Triage.Api.Interfaces;

public interface ITriageService
{
    /// <summary>
    /// Classifies one portal message. <paramref name="modelId"/> overrides the default model
    /// (the eval uses it to compare Sonnet and Haiku).
    /// </summary>
    Task<TriageResult> TriageAsync(TriageRequest request, string? modelId = null, CancellationToken cancellationToken = default);
}
