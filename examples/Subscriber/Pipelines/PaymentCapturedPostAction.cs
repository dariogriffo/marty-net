namespace Subscriber.Pipelines;

using Common.Events;
using Marty.Net.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PaymentCapturedPostAction : IPostProcessor<PaymentRequested>
{
    public Task<OperationResult> Execute(
        PaymentRequested @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine("Post action for requested event");
        return Task.FromResult(result);
    }
}
