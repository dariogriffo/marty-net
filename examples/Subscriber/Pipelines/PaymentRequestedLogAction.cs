namespace Subscriber.Pipelines;

using Common.Events;
using Marty.Net.Contracts;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
