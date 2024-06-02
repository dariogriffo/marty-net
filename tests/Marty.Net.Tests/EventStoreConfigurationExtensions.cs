namespace Marty.Net.Tests;

using System;
using System.Collections.Generic;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class EventStoreConfigurationExtensions
{
    internal static IServiceCollection ConfigureEventStoreTestsDbWithLogging(
        this IServiceCollection services
    )
    {
        Dictionary<string, string> dict =
            new()
            {
                { "Marty.Net:ConnectionString", "esdb://localhost:2113?tls=false" },
                { "Marty.Net:SubscriptionSettings:SubscriptionGroup", "marty-net-tests" }
            };

        IConfiguration conf = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        services
            .AddLogging(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Trace);
                configure.AddConsole();
            })
            .AddSingleton<IStreamNames, StreamNames>()
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
