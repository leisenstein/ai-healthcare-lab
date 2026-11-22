using Microsoft.Extensions.AI;
using Triage.Api.Contracts;

namespace Triage.Api.Interfaces;

/// <summary>Calls the model and returns a validated classification, retrying once on bad JSON.</summary>
public interface ITriageClassifier
{
    Task<TriageClassification> ClassifyAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);
}
