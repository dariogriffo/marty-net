namespace Subscriber.Pipelines;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Marty.Net.Contracts;

public class PaymentRequestedPipelineBehavior : IPipelineBehavior<PaymentRequested>
{
    public async Task<OperationResult> Execute(
        PaymentRequested @event,
        IConsumerContext context,
        Func<PaymentRequested, IConsumerContext, CancellationToken, Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            "Pipeline before for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        OperationResult result = await next(@event, context, cancellationToken);
        Console.WriteLine(
            "Pipeline after for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        return result;
    }
}
