using Microsoft.EntityFrameworkCore;
using Triage.Api.Interfaces;

namespace Triage.Api.Data;

public sealed class AppointmentRepository(AppDb db, TimeProvider time) : IAppointmentRepository
{
    public Task<Appointment?> GetNextAsync(string patientId, CancellationToken cancellationToken = default)
    {
        var now = time.GetLocalNow().DateTime;   // appointments are stored in clinic local time

        return db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId && a.StartsAt > now)
            .OrderBy(a => a.StartsAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
