using Triage.Api.Data;
using Triage.Tests.Fakes;

namespace Triage.Tests.Tools;

public class AppointmentRepositoryTests : IDisposable
{
    // "Now" for every test: Sun 2026-11-22 12:00
    private static readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero));

    private readonly TestDb _testDb = new();
    private readonly AppointmentRepository _repository;

    public AppointmentRepositoryTests() => _repository = new AppointmentRepository(_testDb.Db, Clock);

    public void Dispose() => _testDb.Dispose();

    [Fact]
    public void Seed_has_ten_synthetic_patients_matching_the_messages()
    {
        var ids = _testDb.Db.Patients.Select(p => p.Id).OrderBy(id => id).ToList();

        Assert.Equal(Enumerable.Range(1, 10).Select(i => $"P{i:000}"), ids);
    }

    [Fact]
    public async Task Returns_the_earliest_future_appointment()
    {
        var next = await _repository.GetNextAsync("P001");

        Assert.NotNull(next);
        Assert.Equal(new DateTime(2026, 12, 3, 9, 30, 0), next.StartsAt);
    }

    [Fact]
    public async Task Skips_past_appointments()
    {
        var next = await _repository.GetNextAsync("P003");

        Assert.NotNull(next);
        Assert.Equal(new DateTime(2027, 1, 5, 10, 0, 0), next.StartsAt);
    }

    [Fact]
    public async Task Returns_null_when_nothing_is_scheduled()
    {
        Assert.Null(await _repository.GetNextAsync("P007"));
    }

    [Fact]
    public async Task Returns_null_for_an_unknown_patient()
    {
        Assert.Null(await _repository.GetNextAsync("P999"));
    }
}
