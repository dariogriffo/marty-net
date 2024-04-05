namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Wrappers;

internal interface IHandlesFactory
{
    bool TryGetScopeFor(IEvent @event, out IServiceScope? scope);

    EventHandlerWrapper GetHandlerFor(IEvent @event, IServiceScope scope);

    bool TryGetPipelinesFor(
        IEvent @event,
        IServiceScope scope,
        out List<PipelineBehaviorWrapper>? behaviors
    );

    PreProcessorWrapper[] GetPreProcessorsFor(IEvent @event, IServiceScope scope);

    PostProcessorWrapper[] GetPostProcessorsFor(IEvent @event, IServiceScope scope);

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
