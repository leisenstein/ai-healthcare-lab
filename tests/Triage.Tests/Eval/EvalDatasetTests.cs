using Triage.Api.Contracts;

namespace Triage.Tests.Eval;

/// <summary>Free checks that the labeled data is loadable and sane (no API calls).</summary>
public class EvalDatasetTests
{
    private readonly IReadOnlyList<EvalCase> _cases = EvalDataset.Load();

    [Fact]
    public void Loads_all_30_messages_with_unique_ids() 
    {
        Assert.Equal(30, _cases.Count);
        Assert.Equal(30, _cases.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void Every_message_uses_a_seeded_patient()
    {
        var seeded = Triage.Api.Data.SeedData.Patients.Select(p => p.Id).ToHashSet();

        Assert.All(_cases, c => Assert.Contains(c.PatientId, seeded));
    }

    [Fact]
    public void Every_label_follows_the_needsHuman_rule()
    {
        Assert.All(_cases, c =>
            Assert.Equal(
                c.Expected.Category == TriageCategory.Clinical || c.Expected.Urgency == Urgency.Urgent,
                c.Expected.NeedsHuman));
    }

    [Fact]
    public void Every_category_appears_at_least_once()
    {
        Assert.All(Enum.GetValues<TriageCategory>(), cat => Assert.Contains(_cases, c => c.Expected.Category == cat));
    }
}
