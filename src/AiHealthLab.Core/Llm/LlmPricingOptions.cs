namespace AiHealthLab.Core.Llm;

/// <summary>Price table, bound from the "LlmPricing" section of appsettings.json.</summary>
public sealed class LlmPricingOptions
{
    public Dictionary<string, ModelPrice> Models { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Cost in USD, or null when the model has no price. Matches the exact id first,
    /// then the longest configured id the model starts with (for versioned ids).
    /// </summary>
    public decimal? CostFor(string model, long inputTokens, long outputTokens)
    {
        var price = Find(model);
        if (price is null)
            return null;

        return (inputTokens * price.InputPerMillion + outputTokens * price.OutputPerMillion) / 1_000_000m;
    }

    private ModelPrice? Find(string model)
    {
        foreach (var (key, price) in Models)
            if (string.Equals(key, model, StringComparison.OrdinalIgnoreCase))
                return price;

        return Models
            .Where(kv => model.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(kv => kv.Key.Length)
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }
}
