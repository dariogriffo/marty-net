namespace Subscriber.Pipelines;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Marty.Net.Contracts;

public class PaymentPreActions : IPreProcessor<PaymentRequested>
{
    public Task Execute(
        PaymentRequested @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine("Pre action for requested event {0}", JsonSerializer.Serialize(@event));
        return Task.CompletedTask;
    }
}
