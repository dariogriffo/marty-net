namespace Subscriber.Pipelines;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Marty.Net.Contracts;

public class OpenBehavior<T> : IPipelineBehavior<T>
    where T : IEvent
{
    public async Task<OperationResult> Execute(
        T @event,
        IConsumerContext context,
        Func<T, IConsumerContext, CancellationToken, Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            "1- Pipeline before for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        OperationResult result = await next(@event, context, cancellationToken);
        Console.WriteLine(
            "1- Pipeline after for requested event {0}",
            JsonSerializer.Serialize(@event)
        );
        return result;
    }
}
