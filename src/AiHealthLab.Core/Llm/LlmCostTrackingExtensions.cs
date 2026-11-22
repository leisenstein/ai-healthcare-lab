using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiHealthLab.Core.Llm;

public static class LlmCostTrackingExtensions
{
    /// <summary>Registers the price table and the usage store.</summary>
    public static IServiceCollection AddLlmCostTracking(this IServiceCollection services, LlmPricingOptions pricing)
    {
        services.AddSingleton(pricing);
        services.AddSingleton<IUsageStore, InMemoryUsageStore>();
        return services;
    }

    /// <summary>Adds <see cref="CostLoggingChatClient"/> to a chat client pipeline.</summary>
    public static ChatClientBuilder UseCostLogging(this ChatClientBuilder builder) =>
        builder.Use((inner, services) => new CostLoggingChatClient(
            inner,
            services.GetRequiredService<IUsageStore>(),
            services.GetRequiredService<LlmPricingOptions>(),
            services.GetRequiredService<ILogger<CostLoggingChatClient>>(),
            services.GetService<TimeProvider>()));
}
