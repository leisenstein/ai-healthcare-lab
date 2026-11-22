using Triage.Api.Contracts;
using Triage.Api.Services;

namespace Triage.Tests.Services;

public class TriageResultParserTests
{
    private const string Valid =
        """{"category":"scheduling","urgency":"routine","needsHuman":false,"summary":"Asks to move Thursday's visit","draftReply":"Happy to help reschedule."}""";

    private readonly TriageResultParser _parser = new();

    [Fact]
    public void Parses_valid_json()
    {
        var outcome = _parser.Parse(Valid);

        Assert.True(outcome.Success);
        Assert.Equal(TriageCategory.Scheduling, outcome.Value.Category);
        Assert.Equal(Urgency.Routine, outcome.Value.Urgency);
        Assert.Equal("Asks to move Thursday's visit", outcome.Value.Summary);
    }

    [Fact]
    public void Strips_code_fences()
    {
        var outcome = _parser.Parse($"```json\n{Valid}\n```");

        Assert.True(outcome.Success);
    }

    [Fact]
    public void Ignores_prose_around_the_json()
    {
        var outcome = _parser.Parse($"Here is the triage:\n{Valid}\nLet me know if you need more.");

        Assert.True(outcome.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("I think this is a scheduling request.")]
    public void Rejects_empty_or_non_json(string? raw)
    {
        var outcome = _parser.Parse(raw);

        Assert.False(outcome.Success);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Error));
    }

    [Theory]
    [InlineData("""{"category":"emergency","urgency":"routine","needsHuman":false,"summary":"s","draftReply":"d"}""")]
    [InlineData("""{"category":"refill","urgency":"asap","needsHuman":false,"summary":"s","draftReply":"d"}""")]
    [InlineData("""{"category":3,"urgency":"routine","needsHuman":false,"summary":"s","draftReply":"d"}""")]
    public void Rejects_values_outside_the_enums(string raw)
    {
        Assert.False(_parser.Parse(raw).Success);
    }

    [Theory]
    [InlineData("""{"category":"refill","urgency":"routine","needsHuman":false,"draftReply":"d"}""")]
    [InlineData("""{"category":"refill","urgency":"routine","needsHuman":false,"summary":null,"draftReply":"d"}""")]
    [InlineData("""{"category":"refill","urgency":"routine","needsHuman":false,"summary":"s","draftReply":"  "}""")]
    [InlineData("""{"category":"refill","urgency":"routine","summary":"s","draftReply":"d"}""")]
    public void Rejects_missing_null_or_blank_fields(string raw)
    {
        Assert.False(_parser.Parse(raw).Success);
    }

    [Fact]
    public void Rejects_truncated_json()
    {
        Assert.False(_parser.Parse("""{"category":"refill","urgency":"routine","needsHuman":false,"summary":"s" }""").Success);
    }
}
