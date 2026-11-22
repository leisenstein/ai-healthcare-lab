namespace Triage.Api.Contracts;

/// <summary>Response of POST /triage.</summary>
public sealed record TriageResult(
    string PatientId,
    TriageCategory Category,
    Urgency Urgency,
    bool NeedsHuman,
    string Summary,
    string DraftReply,
    DateTime? NextAppointment);
