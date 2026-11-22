using Triage.Api.Data;

namespace Triage.Api.Interfaces;

public interface IAppointmentRepository
{
    /// <summary>The patient's next appointment after now, or null if there is none.</summary>
    Task<Appointment?> GetNextAsync(string patientId, CancellationToken cancellationToken = default);
}
