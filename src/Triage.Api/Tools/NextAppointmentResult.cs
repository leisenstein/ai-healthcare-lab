namespace Triage.Api.Tools;

/// <summary>
/// What the model sees from get_next_appointment. Only the time and provider:
/// the visit reason stays in the database (minimum necessary).
/// </summary>
public sealed record NextAppointmentResult(bool Found, DateTime? StartsAt, string? Provider, string? Error)
{
    public static NextAppointmentResult None { get; } = new(false, null, null, null);
    public static NextAppointmentResult Denied { get; } = new(false, null, null,
        "Access denied: you can only look up the patient who sent this message.");
}
