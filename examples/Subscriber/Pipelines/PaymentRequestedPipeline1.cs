namespace Subscriber.Pipelines;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Marty.Net.Contracts;

public class PaymentRequestedPipeline1 : IPipelineBehavior<PaymentRequested>
{
    public async Task<OperationResult> Execute(
        PaymentRequested @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            "1- Pipeline before for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        OperationResult result = await next();
        Console.WriteLine(
            "1- Pipeline after for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        return result;
    }
}
