using Triage.Api.Contracts;

namespace Triage.Api.Services;

/// <summary>
/// Safety rules enforced in C#, not only in the prompt: a model's judgment must never be
/// the only check. Rules can only make a result MORE cautious, never less.
/// </summary>
public static class SafetyRules
{
    /// <summary>Any clinical or urgent message must go to a human.</summary>
    public static bool RequiresHuman(TriageCategory category, Urgency urgency) =>
        category == TriageCategory.Clinical || urgency == Urgency.Urgent;

    /// <summary>
    /// Forces NeedsHuman = true when <see cref="RequiresHuman"/> says so.
    /// Never turns NeedsHuman off if the model set it.
    /// </summary>
    public static TriageClassification Apply(TriageClassification classification, out bool overridden)
    {
        overridden = !classification.NeedsHuman
            && RequiresHuman(classification.Category, classification.Urgency);

        return overridden ? classification with { NeedsHuman = true } : classification;
    }
}
