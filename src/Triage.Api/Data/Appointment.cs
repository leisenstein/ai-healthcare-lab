namespace Triage.Api.Data;

/// <summary>A synthetic appointment. StartsAt is clinic local time.</summary>
public sealed class Appointment
{
    public int Id { get; init; }
    public required string PatientId { get; init; }
    public DateTime StartsAt { get; init; }
    public required string Provider { get; init; }
    public required string Reason { get; init; }
}
