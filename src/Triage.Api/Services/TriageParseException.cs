namespace Triage.Api.Services;

/// <summary>Thrown when the model's reply is still invalid after the retry.</summary>
public sealed class TriageParseException(string error)
    : Exception($"Model returned invalid triage JSON after retry: {error}")
{
    public string Error { get; } = error;
}
