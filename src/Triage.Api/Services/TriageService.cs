using Microsoft.Extensions.AI;
using Triage.Api.Contracts;
using Triage.Api.Interfaces;

namespace Triage.Api.Services;

/// <summary>
/// Triage for one message: builds the prompt, gives the model the patient-locked
/// get_next_appointment tool, asks for schema-shaped JSON (validated and retried once by
/// ITriageClassifier), then fills patientId from the request and nextAppointment from the
/// database, never from the model. Finally applies <see cref="SafetyRules"/>.
/// </summary>
public sealed class TriageService(
    ITriageClassifier classifier,
    INextAppointmentToolFactory toolFactory,
    ILogger<TriageService> logger) : ITriageService
{
    public const int MaxOutputTokens = 1024;

    public async Task<TriageResult> TriageAsync(
        TriageRequest request, string? modelId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PatientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Message);

        var patientId = request.PatientId.Trim();
        var tool = toolFactory.CreateFor(patientId);

        var options = new ChatOptions
        {
            ModelId = modelId,
            MaxOutputTokens = MaxOutputTokens,
            Tools = [tool.AsAIFunction()],
            ToolMode = ChatToolMode.Auto,
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                TriageJson.ClassificationSchema,
                schemaName: "triage_classification",
                schemaDescription: "Triage of one patient portal message."),
        };

        var modelResult = await classifier.ClassifyAsync(
            TriagePrompts.Build(patientId, request.Message), options, cancellationToken);

        var classification = SafetyRules.Apply(modelResult, out var overridden);
        if (overridden)
            logger.LogWarning(
                "Safety rule forced needsHuman=true (model said false). category={Category} urgency={Urgency}",
                classification.Category, classification.Urgency);

        return new TriageResult(
            PatientId: patientId,
            Category: classification.Category,
            Urgency: classification.Urgency,
            NeedsHuman: classification.NeedsHuman,
            Summary: classification.Summary,
            DraftReply: classification.DraftReply,
            NextAppointment: tool.LookedUp?.StartsAt);
    }
}
