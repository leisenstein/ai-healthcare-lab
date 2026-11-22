using Triage.Api.Interfaces;

namespace Triage.Api.Tools;

public sealed class NextAppointmentToolFactory(
    IAppointmentRepository repository,
    ILogger<NextAppointmentTool> logger) : INextAppointmentToolFactory
{
    public NextAppointmentTool CreateFor(string requestPatientId) =>
        new(repository, requestPatientId, logger);
}
