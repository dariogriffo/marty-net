using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Marty.Net.Tests.CustomTestFramework", "Marty.Net.Tests")]

namespace Marty.Net.Tests;

using System;
using System.Collections.Generic;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public sealed class CustomTestFramework : XunitTestFramework, IAsyncDisposable
{
    public CustomTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        Container = new ContainerBuilder()
            .WithImage("eventstore/eventstore:24.2")
            .WithPortBinding(1113, 1113)
            .WithPortBinding(2113, 2113)
            .WithEnvironment(
                new Dictionary<string, string>()
                {
                    { "EVENTSTORE_INSECURE", "True" },
                    { "EVENTSTORE_RUN_PROJECTIONS", "All" },
                    { "EVENTSTORE_START_STANDARD_PROJECTIONS", "True" },
                    { "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "True" }
                }
            )
            // Wait until the HTTP endpoint of the container is available.
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113))
            )
            // Build the container configuration.
            .Build();

        Container.StartAsync().GetAwaiter().GetResult();
    }

    public IContainer Container { get; set; }

    private async ValueTask DisposeAsyncCore()
    {
        await Container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync() => DisposeAsyncCore();
}
