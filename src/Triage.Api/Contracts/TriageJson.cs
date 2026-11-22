using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.AI;

namespace Triage.Api.Contracts;

/// <summary>One place for the JSON rules, shared by the parser, the schema and the API.</summary>
public static class TriageJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>JSON schema for <see cref="TriageClassification"/>, sent to the model.</summary>
    public static readonly JsonElement ClassificationSchema = AIJsonUtilities.CreateJsonSchema(
        typeof(TriageClassification),
        description: "Triage of one patient portal message.",
        serializerOptions: Options);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            // Missing fields and nulls in non-nullable strings fail instead of defaulting.
            RespectRequiredConstructorParameters = true,
            RespectNullableAnnotations = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        // "clinical" not "Clinical", and reject numbers like "category": 3.
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.MakeReadOnly();
        return options;
    }
}
