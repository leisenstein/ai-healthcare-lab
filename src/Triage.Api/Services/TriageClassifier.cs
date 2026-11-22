using Microsoft.Extensions.AI;
using Triage.Api.Contracts;
using Triage.Api.Interfaces;

namespace Triage.Api.Services;

/// <summary>
/// Asks the model for a classification. If the JSON fails validation, appends the
/// bad reply plus the validation error to the conversation and retries exactly once.
/// Logs the error but never the patient's message.
/// </summary>
public sealed class TriageClassifier(
    IChatClient chat,
    ITriageResultParser parser,
    ILogger<TriageClassifier> logger) : ITriageClassifier
{
    public async Task<TriageClassification> ClassifyAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var history = messages.ToList();

        var first = await chat.GetResponseAsync(history, options, cancellationToken);
        var outcome = parser.Parse(first.Text);
        if (outcome.Success)
            return outcome.Value;

        logger.LogWarning("Triage JSON failed validation; retrying once. Error: {ValidationError}", outcome.Error);

        history.AddRange(first.Messages);
        history.Add(new ChatMessage(ChatRole.User,
            $"Your previous reply was not valid: {outcome.Error} " +
            "Reply again with only a JSON object that matches the schema. No prose and no code fences."));

        var second = await chat.GetResponseAsync(history, options, cancellationToken);
        var retryOutcome = parser.Parse(second.Text);
        if (retryOutcome.Success)
        {
            logger.LogInformation("Triage JSON retry succeeded.");
            return retryOutcome.Value;
        }

        logger.LogError("Triage JSON still invalid after retry. Error: {ValidationError}", retryOutcome.Error);
        throw new TriageParseException(retryOutcome.Error);
    }
}
