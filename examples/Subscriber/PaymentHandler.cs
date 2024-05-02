namespace Subscriber;

using Common.Events;
using Marty.Net.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PaymentHandler : IEventHandler<PaymentRequested>, IEventHandler<PaymentCaptured>
{
    private static readonly Task<OperationResult> Handled = Task.FromResult(OperationResult.Ok);

    public Task<OperationResult> Handle(
        PaymentCaptured @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine("Handling event captured");
        return Handled;
    }

    public Task<OperationResult> Handle(
        PaymentRequested @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine("Handling event requested");
        return Handled;
    }
}
