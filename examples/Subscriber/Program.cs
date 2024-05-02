using Common.Aggregates;
using Marty.Net;
using Marty.Net.Aggregates.Contracts;
using Marty.Net.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Subscriber;
using Subscriber.Pipelines;
using System;

ServiceCollection services = new();
services
    .ConfigureEventStoreDbWithLogging()
    .AddMarty(
        sp => sp.GetEventStoreSettings(),
        configuration =>
        {
            configuration.AssembliesToScanForHandlers = [typeof(PaymentHandler).Assembly];
            configuration
                .AddBehavior<PaymentRequestedPipelineBehavior>()
                .AddBehavior<PaymentRequestedPipeline1>()
                .AddOpenBehavior(typeof(OpenBehavior<>));
        }
    )
    .AddMartyAggregates();

ServiceProvider provider = services.BuildServiceProvider();
IAggregateStore aggregateStore = provider.GetRequiredService<IAggregateStore>();
await aggregateStore.SubscribeTo<Payment>();
Console.ReadKey();
