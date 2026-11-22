using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AiHealthLab.Core.Llm;

/// <summary>
/// Wraps any IChatClient and, for every model call, logs and records the model,
/// tokens in/out (read from the response's usage, never estimated), latency and cost.
/// Place it innermost in the pipeline so each tool round-trip is counted as its own call.
/// </summary>
public sealed class CostLoggingChatClient(
    IChatClient innerClient,
    IUsageStore store,
    LlmPricingOptions pricing,
    ILogger<CostLoggingChatClient> logger,
    TimeProvider? timeProvider = null) : DelegatingChatClient(innerClient)
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var start = _time.GetTimestamp();
        ChatResponse response;
        try
        {
            response = await base.GetResponseAsync(messages, options, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "LLM call failed. model={Model} latencyMs={LatencyMs}",
                ResolveModel(null, options), _time.GetElapsedTime(start).TotalMilliseconds);
            throw;
        }

        Record(response, options, _time.GetElapsedTime(start));
        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var start = _time.GetTimestamp();
        var updates = new List<ChatResponseUpdate>();

        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            updates.Add(update);
            yield return update;
        }

        Record(updates.ToChatResponse(), options, _time.GetElapsedTime(start));
    }

    private void Record(ChatResponse response, ChatOptions? options, TimeSpan latency)
    {
        var model = ResolveModel(response, options);

        if (response.Usage is null)
            logger.LogWarning("LLM response had no usage data; tokens recorded as 0. model={Model}", model);

        var inputTokens = response.Usage?.InputTokenCount ?? 0;
        var outputTokens = response.Usage?.OutputTokenCount ?? 0;

        var cost = pricing.CostFor(model, inputTokens, outputTokens);
        if (cost is null)
            logger.LogWarning("No price configured for model {Model}; cost recorded as 0. Add it to LlmPricing.", model);

        var record = new ModelCallRecord(_time.GetUtcNow(), model, inputTokens, outputTokens, latency, cost ?? 0m);
        store.Add(record);

        logger.LogInformation(
            "LLM call model={Model} tokensIn={InputTokens} tokensOut={OutputTokens} latencyMs={LatencyMs:F0} costUsd={CostUsd:F6}",
            record.Model, record.InputTokens, record.OutputTokens, record.Latency.TotalMilliseconds, record.CostUsd);
    }

    private string ResolveModel(ChatResponse? response, ChatOptions? options) =>
        response?.ModelId
        ?? options?.ModelId
        ?? this.GetService<ChatClientMetadata>()?.DefaultModelId
        ?? "unknown";
}
