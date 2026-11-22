using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Triage.Api.Contracts;
using Triage.Api.Services;
using Triage.Tests.Fakes;

namespace Triage.Tests.Services;

public class TriageClassifierTests
{
    private const string Valid =
        """{"category":"refill","urgency":"soon","needsHuman":false,"summary":"Needs lisinopril refill","draftReply":"We'll send it to your pharmacy."}""";
    private const string Invalid = "Sure! This looks like a refill request.";

    private static readonly ChatMessage[] Prompt =
        [new(ChatRole.System, "triage"), new(ChatRole.User, "can i get a refill on my lisinopril")];

    private static (TriageClassifier Classifier, FakeChatClient Chat, ListLogger<TriageClassifier> Log) Create(params string[] replies)
    {
        var chat = new FakeChatClient(replies);
        var log = new ListLogger<TriageClassifier>();
        return (new TriageClassifier(chat, new TriageResultParser(), log), chat, log);
    }

    [Fact]
    public async Task Valid_first_reply_makes_one_call()
    {
        var (classifier, chat, log) = Create(Valid);

        var result = await classifier.ClassifyAsync(Prompt);

        Assert.Equal(TriageCategory.Refill, result.Category);
        Assert.Single(chat.Calls);
        Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task Invalid_reply_is_retried_once_with_the_error_appended()
    {
        var (classifier, chat, _) = Create(Invalid, Valid);

        var result = await classifier.ClassifyAsync(Prompt);

        Assert.Equal(TriageCategory.Refill, result.Category);
        Assert.Equal(2, chat.Calls.Count);

        var retry = chat.Calls[1];
        Assert.Equal(Prompt.Length + 2, retry.Count);            // original + bad reply + correction
        Assert.Equal(ChatRole.Assistant, retry[^2].Role);
        Assert.Equal(Invalid, retry[^2].Text);
        Assert.Equal(ChatRole.User, retry[^1].Role);
        Assert.Contains("No JSON object was found", retry[^1].Text);
    }

    [Fact]
    public async Task Retry_is_logged_as_a_warning_without_the_patient_message()
    {
        var (classifier, _, log) = Create(Invalid, Valid);

        await classifier.ClassifyAsync(Prompt);

        var warning = Assert.Single(log.Entries, e => e.Level == LogLevel.Warning);
        Assert.Contains("retrying", warning.Message);
        Assert.DoesNotContain("lisinopril", warning.Message);
    }

    [Fact]
    public async Task Two_invalid_replies_throw_and_stop()
    {
        var (classifier, chat, log) = Create(Invalid, Invalid);

        await Assert.ThrowsAsync<TriageParseException>(() => classifier.ClassifyAsync(Prompt));

        Assert.Equal(2, chat.Calls.Count);
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Caller_message_list_is_not_modified()
    {
        var (classifier, _, _) = Create(Invalid, Valid);
        var messages = Prompt.ToList();

        await classifier.ClassifyAsync(messages);

        Assert.Equal(Prompt.Length, messages.Count);
    }
}
