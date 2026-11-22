namespace AiHealthLab.Core.Llm;

/// <summary>Where model call records go. In memory for now; swap for a database later.</summary>
public interface IUsageStore
{
    void Add(ModelCallRecord record);
    IReadOnlyList<ModelCallRecord> All();
    UsageSummary Summarize();
}
