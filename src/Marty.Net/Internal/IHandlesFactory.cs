namespace Marty.Net.Internal;

using Contracts;
using System;
using System.Collections.Generic;
using Wrappers;

internal record ExecutionPlan
{
    public ExecutionPlan(PipelineBehaviorWrapper[] behaviors)
    {
        Behaviors = behaviors;
    }

    internal PipelineBehaviorWrapper[] Behaviors { get; init; } = null!;
}

internal interface IHandlesFactory
{
    ExecutionPlan? TryGetExecutionPlanFor(IEvent @event);

    bool TryGetPostAppendEventActionsFor(
        IEvent @event,
        IServiceProvider serviceProvider,
        out List<PostAppendEventActionWrapper>? actions
    );

    bool TryGetPreAppendEventActionFor(
        IEvent @event,
        IServiceProvider serviceProvider,
        out List<PreAppendEventActionWrapper>? actions
    );
}
