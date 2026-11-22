namespace AiHealthLab.Core.Llm;

/// <summary>USD per 1 million tokens for one model.</summary>
public sealed class ModelPrice
{
    public decimal InputPerMillion { get; init; }
    public decimal OutputPerMillion { get; init; }
}
