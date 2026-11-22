using Microsoft.Extensions.AI;

namespace Triage.Tests.Fakes;

/// <summary>Returns canned replies in order and records every call. Costs nothing.</summary>
public sealed class FakeChatClient(params string[] replies) : IChatClient
{
    private readonly Queue<string> _replies = new(replies);

    public List<List<ChatMessage>> Calls { get; } = [];

    /// <summary>Usage to attach to every response (null = provider sent none).</summary>
    public UsageDetails? Usage { get; set; }

    /// <summary>Model id to put on every response.</summary>
    public string? ModelId { get; set; }

    /// <summary>When set, every call throws this instead of replying.</summary>
    public Exception? Throw { get; set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Calls.Add(messages.ToList());
        if (Throw is not null)
            throw Throw;

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _replies.Dequeue()))
        {
            Usage = Usage,
            ModelId = ModelId,
        });
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
