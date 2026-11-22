using Triage.Api.Contracts;
using Triage.Api.Services;

namespace Triage.Tests.Services;

/// <summary>
/// The Week 1 "done when" test: clinical and urgent ALWAYS set needsHuman.
/// Checks every category x urgency x model-answer combination (24 cases).
/// </summary>
public class SafetyRulesTests
{
    public static TheoryData<TriageCategory, Urgency, bool> AllCombinations()
    {
        var data = new TheoryData<TriageCategory, Urgency, bool>();
        foreach (var category in Enum.GetValues<TriageCategory>())
            foreach (var urgency in Enum.GetValues<Urgency>())
                foreach (var modelSaid in new[] { true, false })
                    data.Add(category, urgency, modelSaid);
        return data;
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Clinical_or_urgent_always_needs_a_human_and_nothing_is_ever_lowered(
        TriageCategory category, Urgency urgency, bool modelSaidNeedsHuman)
    {
        var input = new TriageClassification(category, urgency, modelSaidNeedsHuman, "s", "d");

        var result = SafetyRules.Apply(input, out var overridden);

        var mustBeHuman = category == TriageCategory.Clinical || urgency == Urgency.Urgent;
        Assert.Equal(mustBeHuman || modelSaidNeedsHuman, result.NeedsHuman);
        Assert.Equal(mustBeHuman && !modelSaidNeedsHuman, overridden);

        // Nothing else about the model's answer changes.
        Assert.Equal(input with { NeedsHuman = result.NeedsHuman }, result);
    }
}
