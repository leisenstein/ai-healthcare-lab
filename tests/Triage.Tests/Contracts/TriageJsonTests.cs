using System.Text.Json;
using Triage.Api.Contracts;

namespace Triage.Tests.Contracts;

public class TriageJsonTests
{
    [Fact]
    public void Result_serializes_with_camelCase_names_and_lowercase_enums()
    {
        var result = new TriageResult("P001", TriageCategory.Clinical, Urgency.Urgent, true,
            "Chest pain", "Please call 911.", new DateTime(2026, 12, 3, 9, 30, 0));

        var json = JsonSerializer.Serialize(result, TriageJson.Options);

        Assert.Contains("\"patientId\":\"P001\"", json);
        Assert.Contains("\"category\":\"clinical\"", json);
        Assert.Contains("\"urgency\":\"urgent\"", json);
        Assert.Contains("\"needsHuman\":true", json);
        Assert.Contains("\"nextAppointment\":\"2026-12-03T09:30:00\"", json);
    }

    [Fact]
    public void Schema_lists_every_allowed_category_and_urgency()
    {
        var schema = TriageJson.ClassificationSchema.GetRawText();

        foreach (var value in new[] { "refill", "scheduling", "billing", "clinical", "routine", "soon", "urgent" })
            Assert.Contains($"\"{value}\"", schema);
    }

    [Fact]
    public void Schema_does_not_let_the_model_set_patientId_or_nextAppointment()
    {
        var schema = TriageJson.ClassificationSchema.GetRawText();

        Assert.DoesNotContain("patientId", schema);
        Assert.DoesNotContain("nextAppointment", schema);
    }
}
