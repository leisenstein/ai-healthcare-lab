using System.ComponentModel;
using Microsoft.Extensions.AI;
using Triage.Api.Data;
using Triage.Api.Interfaces;

namespace Triage.Api.Tools;

/// <summary>
/// get_next_appointment, created per request and locked to that request's patient.
/// If the model asks for any other patient ID, it gets "access denied" and no data.
/// It also remembers what the database actually returned, so TriageService can fill
/// nextAppointment from the database instead of trusting the model's text.
/// </summary>
public sealed class NextAppointmentTool(
    IAppointmentRepository repository,
    string allowedPatientId,
    ILogger<NextAppointmentTool> logger)
{
    public const string Name = "get_next_appointment";

    /// <summary>True once the model has called the tool for the allowed patient.</summary>
    public bool WasCalled { get; private set; }

    /// <summary>The appointment the database returned (null if none or never called).</summary>
    public Appointment? LookedUp { get; private set; }

    public AIFunction AsAIFunction() => AIFunctionFactory.Create(
        GetNextAppointmentAsync,
        Name,
        "Returns the next upcoming appointment (time and provider) for the patient who sent this message. " +
        "found=false means they have none scheduled.");

    public async Task<NextAppointmentResult> GetNextAppointmentAsync(
        [Description("The patient ID from the request, e.g. P001.")] string patientId,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(patientId?.Trim(), allowedPatientId, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Blocked {Tool} call for a different patient than the request's.", Name);
            return NextAppointmentResult.Denied;
        }

        WasCalled = true;
        LookedUp = await repository.GetNextAsync(allowedPatientId, cancellationToken);

        return LookedUp is null
            ? NextAppointmentResult.None
            : new NextAppointmentResult(true, LookedUp.StartsAt, LookedUp.Provider, null);
    }
}
