namespace Triage.Api.Contracts;

/// <summary>What the message is about. Serialized as lowercase strings ("refill", "clinical", ...).</summary>
public enum TriageCategory
{
    Refill,
    Scheduling,
    Billing,
    Clinical
}
