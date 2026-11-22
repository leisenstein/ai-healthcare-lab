namespace Triage.Api.Contracts;

/// <summary>Body of POST /triage.</summary>
public sealed record TriageRequest(string PatientId, string Message);
