namespace Triage.Tests.Eval;

public class EvalSettingsTests
{
    private static EvalSettings With(string? models = null, string? limit = null) =>
        EvalSettings.From(name => name switch
        {
            "EVAL_MODELS" => models,
            "EVAL_LIMIT" => limit,
            _ => null,
        });

    [Fact]
    public void No_variables_means_the_full_sonnet_and_haiku_run()
    {
        var s = With();

        Assert.Equal(new[] { "claude-sonnet-5-5", "claude-haiku-5-5" }, s.Models);
        Assert.Null(s.Limit);
        Assert.True(s.IsFullRun(30));
    }

    [Theory]
    [InlineData("haiku", "claude-haiku-5-5")]
    [InlineData("HAIKU", "claude-haiku-5-5")]
    [InlineData(" sonnet ", "claude-sonnet-5-5")]
    [InlineData("opus", "claude-opus-5-5")]
    [InlineData("claude-haiku-5-5", "claude-haiku-5-5")]
    public void Short_names_and_full_ids_both_work(string input, string expected)
    {
        Assert.Equal(new[] { expected }, With(models: input).Models);
    }

    [Fact]
    public void Comma_list_keeps_order_and_drops_duplicates()
    {
        Assert.Equal(new[] { "claude-haiku-5-5", "claude-sonnet-5-5" }, With(models: "haiku, sonnet, haiku").Models);
    }

    [Fact]
    public void Limit_picks_the_first_n_messages()
    {
        var cases = EvalDataset.Load();
        var s = With(limit: "10");

        var picked = s.Pick(cases);

        Assert.Equal(10, picked.Count);
        Assert.Equal("m01", picked[0].Id);
        Assert.False(s.IsFullRun(cases.Count));
    }

    [Fact]
    public void Limit_bigger_than_the_dataset_is_a_full_run()
    {
        var cases = EvalDataset.Load();
        var s = With(limit: "100");

        Assert.Equal(cases.Count, s.Pick(cases).Count);
        Assert.True(s.IsFullRun(cases.Count));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("ten")]
    public void Bad_limit_fails_fast_before_spending_money(string limit)
    {
        Assert.Throws<ArgumentException>(() => With(limit: limit));
    }
}
