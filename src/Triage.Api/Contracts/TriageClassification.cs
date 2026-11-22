namespace Triage.Api.Contracts;

/// <summary>
/// The part of the answer the MODEL produces. PatientId and NextAppointment are
/// deliberately not here: C# fills those from the request and the database so the
/// model can't invent them.
/// </summary>
public sealed record TriageClassification(
    TriageCategory Category,
    Urgency Urgency,
    bool NeedsHuman,
    string Summary,
    string DraftReply);
