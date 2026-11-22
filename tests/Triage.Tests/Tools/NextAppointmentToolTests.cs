using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Triage.Api.Data;
using Triage.Api.Tools;
using Triage.Tests.Fakes;

namespace Triage.Tests.Tools;

public class NextAppointmentToolTests : IDisposable
{
    private static readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero));

    private readonly TestDb _testDb = new();
    private readonly ListLogger<NextAppointmentTool> _log = new();
    private readonly NextAppointmentToolFactory _factory;

    public NextAppointmentToolTests() =>
        _factory = new NextAppointmentToolFactory(new AppointmentRepository(_testDb.Db, Clock), _log);

    public void Dispose() => _testDb.Dispose();

    [Fact]
    public async Task Returns_the_request_patients_next_appointment()
    {
        var tool = _factory.CreateFor("P001");

        var result = await tool.GetNextAppointmentAsync("P001");

        Assert.True(result.Found);
        Assert.Equal(new DateTime(2026, 12, 3, 9, 30, 0), result.StartsAt);
        Assert.Equal("Dr. Lane", result.Provider);
        Assert.True(tool.WasCalled);
        Assert.Equal(result.StartsAt, tool.LookedUp?.StartsAt);
    }

    [Fact]
    public async Task Another_patients_id_is_denied_and_returns_no_data()
    {
        var tool = _factory.CreateFor("P001");

        var result = await tool.GetNextAppointmentAsync("P002");

        Assert.False(result.Found);
        Assert.Null(result.StartsAt);
        Assert.Null(result.Provider);
        Assert.Contains("Access denied", result.Error);
        Assert.False(tool.WasCalled);
        Assert.Null(tool.LookedUp);
        Assert.Contains(_log.Entries, e => e.Level == LogLevel.Warning);
    }

    [Theory]
    [InlineData("p001")]
    [InlineData(" P001 ")]
    public async Task Same_patient_id_with_different_case_or_spaces_is_allowed(string patientId)
    {
        var result = await _factory.CreateFor("P001").GetNextAppointmentAsync(patientId);

        Assert.True(result.Found);
    }

    [Fact]
    public async Task No_appointment_returns_found_false_without_an_error()
    {
        var tool = _factory.CreateFor("P007");

        var result = await tool.GetNextAppointmentAsync("P007");

        Assert.False(result.Found);
        Assert.Null(result.Error);
        Assert.True(tool.WasCalled);
        Assert.Null(tool.LookedUp);
    }

    [Fact]
    public void Result_never_includes_the_visit_reason()
    {
        Assert.DoesNotContain(typeof(NextAppointmentResult).GetProperties(), p => p.Name == "Reason");
    }

    [Fact]
    public void AIFunction_is_named_get_next_appointment_and_asks_for_patientId()
    {
        var function = _factory.CreateFor("P001").AsAIFunction();

        Assert.Equal("get_next_appointment", function.Name);
        Assert.Contains("patientId", function.JsonSchema.GetRawText());
        Assert.DoesNotContain("cancellationToken", function.JsonSchema.GetRawText());
    }

    [Fact]
    public async Task AIFunction_enforces_the_lock_when_the_model_calls_it()
    {
        var tool = _factory.CreateFor("P001");
        var function = tool.AsAIFunction();

        var result = await function.InvokeAsync(new AIFunctionArguments { ["patientId"] = "P002" });

        Assert.Contains("Access denied", result?.ToString());
        Assert.Null(tool.LookedUp);
    }
}
