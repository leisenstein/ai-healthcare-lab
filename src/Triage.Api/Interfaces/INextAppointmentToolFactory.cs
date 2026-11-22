using Triage.Api.Tools;

namespace Triage.Api.Interfaces;

public interface INextAppointmentToolFactory
{
    /// <summary>A tool that can only look up <paramref name="requestPatientId"/>.</summary>
    NextAppointmentTool CreateFor(string requestPatientId);
}
