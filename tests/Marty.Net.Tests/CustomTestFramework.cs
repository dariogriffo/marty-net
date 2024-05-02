using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Marty.Net.Tests.CustomTestFramework", "Marty.Net.Tests")]

namespace Marty.Net.Tests;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System;
using System.Collections.Generic;

public sealed class CustomTestFramework : XunitTestFramework, IDisposable
{
    public CustomTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        Container = new ContainerBuilder()
            // Set the image for the container to "testcontainers/helloworld:1.1.0".
            .WithImage("eventstore/eventstore:23.6.0-buster-slim")
            // Bind port 8080 of the container to a random port on the host.
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

    public new void Dispose()
    {
        Container.StopAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
