using AiHealthLab.Core.Llm;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Triage.Tests.Fakes;

namespace Triage.Tests.Core;

public class CostLoggingChatClientTests
{
    private static readonly LlmPricingOptions Pricing = new()
    {
        Models =
        {
            ["claude-sonnet-5-5"] = new ModelPrice { InputPerMillion = 2.00m, OutputPerMillion = 10.00m },
            ["claude-haiku-5-5"] = new ModelPrice { InputPerMillion = 0.10m, OutputPerMillion = 0.50m },
        }
    };

    private static readonly ChatMessage[] Prompt = [new(ChatRole.User, "my lisinopril refill please")];

    private static (CostLoggingChatClient Client, FakeChatClient Inner, InMemoryUsageStore Store, ListLogger<CostLoggingChatClient> Log) Create()
    {
        var inner = new FakeChatClient("ok", "ok", "ok")
        {
            ModelId = "claude-sonnet-5-5",
            Usage = new UsageDetails { InputTokenCount = 1_000, OutputTokenCount = 500 },
        };
        var store = new InMemoryUsageStore();
        var log = new ListLogger<CostLoggingChatClient>();
        return (new CostLoggingChatClient(inner, store, Pricing, log), inner, store, log);
    }

    [Fact]
    public async Task Records_model_tokens_latency_and_cost_from_the_response()
    {
        var (client, _, store, _) = Create();

        await client.GetResponseAsync(Prompt);

        var record = Assert.Single(store.All());
        Assert.Equal("claude-sonnet-5-5", record.Model);
        Assert.Equal(1_000, record.InputTokens);
        Assert.Equal(500, record.OutputTokens);
        Assert.Equal(0.007m, record.CostUsd);   // 1000 * $2/1M + 500 * $10/1M
        Assert.True(record.Latency >= TimeSpan.Zero);
    }

    [Fact]
    public async Task Logs_every_field_but_not_the_message_text()
    {
        var (client, _, _, log) = Create();

        await client.GetResponseAsync(Prompt);

        var entry = Assert.Single(log.Entries, e => e.Level == LogLevel.Information);
        Assert.Contains("model=claude-sonnet-5-5", entry.Message);
        Assert.Contains("tokensIn=1000", entry.Message);
        Assert.Contains("tokensOut=500", entry.Message);
        Assert.Contains("latencyMs=", entry.Message);
        Assert.Contains("costUsd=0.007000", entry.Message);
        Assert.DoesNotContain("lisinopril", entry.Message);
    }

    [Fact]
    public async Task Uses_options_model_when_response_has_none()
    {
        var (client, inner, store, _) = Create();
        inner.ModelId = null;

        await client.GetResponseAsync(Prompt, new ChatOptions { ModelId = "claude-haiku-5-5" });

        var record = Assert.Single(store.All());
        Assert.Equal("claude-haiku-5-5", record.Model);
        Assert.Equal(0.00035m, record.CostUsd);   // 1000 * $0.10/1M + 500 * $0.50/1M
    }

    [Fact]
    public async Task Unknown_model_costs_zero_and_warns()
    {
        var (client, inner, store, log) = Create();
        inner.ModelId = "some-other-model";

        await client.GetResponseAsync(Prompt);

        Assert.Equal(0m, Assert.Single(store.All()).CostUsd);
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("No price configured"));
    }

    [Fact]
    public async Task Missing_usage_records_zero_tokens_and_warns()
    {
        var (client, inner, store, log) = Create();
        inner.Usage = null;

        await client.GetResponseAsync(Prompt);

        var record = Assert.Single(store.All());
        Assert.Equal(0, record.InputTokens);
        Assert.Equal(0, record.OutputTokens);
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("no usage data"));
    }

    [Fact]
    public async Task Failed_call_is_logged_rethrown_and_not_recorded()
    {
        var (client, inner, store, log) = Create();
        inner.Throw = new HttpRequestException("boom");

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetResponseAsync(Prompt));

        Assert.Empty(store.All());
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("LLM call failed"));
    }
}
