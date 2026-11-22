using System.Text.Json;
using Triage.Api.Contracts;
using Triage.Api.Interfaces;

namespace Triage.Api.Services;

/// <summary>
/// Validates model output with System.Text.Json. Tolerates code fences and stray
/// prose around the JSON object, but nothing inside it.
/// </summary>
public sealed class TriageResultParser : ITriageResultParser
{
    public ParseOutcome Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return ParseOutcome.Fail("The reply was empty.");

        var json = ExtractJsonObject(raw);
        if (json is null)
            return ParseOutcome.Fail("No JSON object was found in the reply.");

        TriageClassification? value;
        try
        {
            value = JsonSerializer.Deserialize<TriageClassification>(json, TriageJson.Options);
        }
        catch (JsonException ex)
        {
            return ParseOutcome.Fail($"Invalid JSON: {ex.Message}");
        }

        if (value is null)
            return ParseOutcome.Fail("The JSON was null.");
        if (!Enum.IsDefined(value.Category))
            return ParseOutcome.Fail("category must be one of refill, scheduling, billing, clinical.");
        if (!Enum.IsDefined(value.Urgency))
            return ParseOutcome.Fail("urgency must be one of routine, soon, urgent.");
        if (string.IsNullOrWhiteSpace(value.Summary))
            return ParseOutcome.Fail("summary must not be empty.");
        if (string.IsNullOrWhiteSpace(value.DraftReply))
            return ParseOutcome.Fail("draftReply must not be empty.");

        return ParseOutcome.Ok(value);
    }

    /// <summary>Strips ```json fences and any prose before the first { or after the last }.</summary>
    private static string? ExtractJsonObject(string raw)
    {
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw[start..(end + 1)] : null;
    }
}
