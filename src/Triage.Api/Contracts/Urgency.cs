namespace Triage.Api.Contracts;

/// <summary>How fast the front desk should act. Serialized as lowercase strings.</summary>
public enum Urgency
{
    Routine,
    Soon,
    Urgent
}
