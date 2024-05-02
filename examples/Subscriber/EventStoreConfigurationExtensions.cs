namespace Subscriber;

using Marty.Net.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

internal static class EventStoreConfigurationExtensions
{
    internal static IServiceCollection ConfigureEventStoreDbWithLogging(
        this IServiceCollection services
    )
    {
        Dictionary<string, string> dict =
            new()
            {
                { "Marty.Net:ConnectionString", "esdb://localhost:2113?tls=false" },
                { "Marty.Net:SubscriptionSettings:SubscriptionGroup", "subscriber-example" },
                { "Marty.Net:ResolveEvents", "true" },
                { "Marty.Net:ReconnectOnSubscriptionDropped", "true" }
            };

        IConfiguration conf = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        services
            .AddLogging(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Trace);
                configure.AddConsole();
            })
            .AddSingleton(conf);

        return services;
    }

    internal static EventStoreSettings GetEventStoreSettings(this IServiceProvider sp)
    {
        return sp.GetRequiredService<IConfiguration>()
            .GetSection("Marty.Net")
            .Get<EventStoreSettings>()!;
    }
}
