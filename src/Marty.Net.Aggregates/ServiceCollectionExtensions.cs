namespace Marty.Net.Contracts;

using Aggregates.Contracts;
using Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

/// <summary>
///     The integration point with Marty.Net Aggregates with the <see cref="Microsoft.Extensions.DependencyInjection" />
///     framework
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Aggregates to Marty.Net allowing to configuring options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the registration happens.</param>
    /// <param name="actionsConfigurator">The configuration for the Dependency Injection.</param>
    /// <returns>.</returns>
    public static IServiceCollection AddMartyAggregates(
        this IServiceCollection services,
        Action<AggregatesConfiguration>? actionsConfigurator = default
    )
    {
        AggregatesConfiguration configuration = new();

        Action<AggregatesConfiguration> decoratorConfigurator = config =>
        {
            actionsConfigurator?.Invoke(config);
            config.AggregateStreamResolver ??= typeof(AggregateByTypeResolver);
        };

        decoratorConfigurator.Invoke(configuration);
        services.Configure(decoratorConfigurator);

        services.AddSingleton(configuration.AggregateStreamResolver!);

        services.AddSingleton<IAggregateStreamResolver>(sp =>
            (IAggregateStreamResolver)
                sp.GetRequiredService(
                    sp.GetRequiredService<
                        IOptions<AggregatesConfiguration>
                    >().Value.AggregateStreamResolver!
                )
        );

        services.AddScoped<IAggregateStore, AggregatesStore>();

        return services;
    }
}
