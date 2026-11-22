namespace Triage.Api.Data;

/// <summary>A synthetic patient. No real patient data, ever.</summary>
public sealed class Patient
{
    public required string Id { get; init; }          // "P001"
    public required string DisplayName { get; init; } // invented name
    public List<Appointment> Appointments { get; init; } = [];
}
