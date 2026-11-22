using Microsoft.Extensions.AI;

namespace Triage.Tests.Fakes;

/// <summary>
/// Plays back a script of model turns (text or tool calls) and records what it was sent.
/// Wrap it with UseFunctionInvocation() to exercise real tool calling for free.
/// </summary>
public sealed class ScriptedChatClient : IChatClient
{
    private readonly Queue<ChatMessage> _turns = new();

    public List<List<ChatMessage>> Calls { get; } = [];
    public List<ChatOptions?> Options { get; } = [];

    public ScriptedChatClient Text(string text)
    {
        _turns.Enqueue(new ChatMessage(ChatRole.Assistant, text));
        return this;
    }

    public ScriptedChatClient ToolCall(string name, string patientId)
    {
        _turns.Enqueue(new ChatMessage(ChatRole.Assistant,
            [new FunctionCallContent($"call_{_turns.Count}", name, new Dictionary<string, object?> { ["patientId"] = patientId })]));
        return this;
    }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Calls.Add(messages.ToList());
        Options.Add(options);
        return Task.FromResult(new ChatResponse(_turns.Dequeue()));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
