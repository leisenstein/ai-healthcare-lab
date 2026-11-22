using Triage.Api.Services;

namespace Triage.Api.Interfaces;

/// <summary>Turns raw model text into a validated <c>TriageClassification</c>.</summary>
public interface ITriageResultParser
{
    ParseOutcome Parse(string? raw);
}
