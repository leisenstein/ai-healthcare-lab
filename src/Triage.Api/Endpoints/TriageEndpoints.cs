using AiHealthLab.Core.Llm;
using Triage.Api.Contracts;
using Triage.Api.Interfaces;
using Triage.Api.Services;

namespace Triage.Api.Endpoints;

public static class TriageEndpoints
{
    private const int MaxMessageLength = 4000;

    public static IEndpointRouteBuilder MapTriageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/triage", async (TriageRequest request, ITriageService triage, CancellationToken ct) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request.PatientId))
                errors["patientId"] = ["PatientId is required."];
            if (string.IsNullOrWhiteSpace(request.Message))
                errors["message"] = ["Message is required."];
            else if (request.Message.Length > MaxMessageLength)
                errors["message"] = [$"Message must be {MaxMessageLength} characters or fewer."];

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            try
            {
                var result = await triage.TriageAsync(request, cancellationToken: ct);
                return Results.Ok(result);
            }
            catch (TriageParseException)
            {
                return Results.Problem(
                    title: "Triage model failed",
                    detail: "The model did not return a valid triage result.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapGet("/stats", (IUsageStore usage) => Results.Ok(usage.Summarize()));

        return app;
    }
}