using AiHealthLab.Core.Llm;

namespace Triage.Tests.Core;

public class LlmPricingOptionsTests
{
    private static readonly LlmPricingOptions Pricing = new()
    {
        Models = { ["claude-sonnet-5-5"] = new ModelPrice { InputPerMillion = 2.00m, OutputPerMillion = 10.00m } }
    };

    [Fact]
    public void Exact_match_prices_per_million_tokens() =>
        Assert.Equal(12.00m, Pricing.CostFor("claude-sonnet-5-5", 1_000_000, 1_000_000));

    [Fact]
    public void Match_ignores_case() =>
        Assert.Equal(2.00m, Pricing.CostFor("Claude-Sonnet-5-5", 1_000_000, 0));

    [Fact]
    public void Versioned_model_id_falls_back_to_its_base_price() =>
        Assert.Equal(10.00m, Pricing.CostFor("claude-sonnet-5-5-preview", 0, 1_000_000));

    [Fact]
    public void Unknown_model_returns_null() =>
        Assert.Null(Pricing.CostFor("claude-haiku-5-5", 100, 100));
}
