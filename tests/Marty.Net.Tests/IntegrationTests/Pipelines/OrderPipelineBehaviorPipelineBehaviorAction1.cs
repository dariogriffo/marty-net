﻿namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders.v2;
using System;
using System.Threading;
using System.Threading.Tasks;

public class OrderPipelineBehaviorPipelineBehaviorAction1(ICounter counter)
    : IPipelineBehavior<OrderEventCancelled>
{
    public async Task<OperationResult> Execute(
        OrderEventCancelled @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("a");
        counter.Touch();
        OperationResult result = await next();
        counter.Touch();
        return result;
    }
}
