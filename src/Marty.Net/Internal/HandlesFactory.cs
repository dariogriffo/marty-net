namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Wrappers;

internal sealed class HandlesFactory : IHandlesFactory
{
    private readonly HashSet<Type> _registeredHandlers;
    private readonly IServiceProvider _serviceProvider;

    public HandlesFactory(
        IServiceProvider serviceProvider,
        HandlersAndEventTypes handlersAndEventTypes
    )
    {
        _serviceProvider = serviceProvider;
        _registeredHandlers = handlersAndEventTypes.RegisteredEventsAndHandlers;
    }

    public bool TryGetScopeFor(IEvent @event, out IServiceScope? scope)
    {
        scope = default;
        if (!_registeredHandlers.TryGetValue(@event.GetType(), out _))
        {
            return false;
        }

        scope = _serviceProvider.CreateScope();
        return true;
    }

    public EventHandlerWrapper GetHandlerFor(IEvent @event, IServiceScope scope)
    {
        Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(@event.GetType());
        Type handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        object? service = scope.ServiceProvider.GetService(handlerType);
        return (Activator.CreateInstance(wrapperType, service) as EventHandlerWrapper)!;
    }

    public bool TryGetPipelinesFor(
        IEvent @event,
        IServiceScope scope,
        out List<PipelineBehaviorWrapper>? behaviors
    )
    {
        Type wrapperType = typeof(PipelineBehaviorWrapper<>).MakeGenericType(@event.GetType());
        Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(@event.GetType());

        behaviors = scope
            .ServiceProvider.GetServices(behaviorType)
            .Where(x => x is not null)
            .Select(service =>
                (Activator.CreateInstance(wrapperType, service) as PipelineBehaviorWrapper)!
            )
            .ToList();

        return behaviors.Count > 0;
    }

    public PreProcessorWrapper[] GetPreProcessorsFor(IEvent @event, IServiceScope scope)
    {
        Type wrapperType = typeof(PreProcessorWrapper<>).MakeGenericType(@event.GetType());
        Type processorType = typeof(IPreProcessor<>).MakeGenericType(@event.GetType());

        return scope
            .ServiceProvider.GetServices(processorType)
            .Where(x => x is not null)
            .Select(service =>
                (Activator.CreateInstance(wrapperType, service) as PreProcessorWrapper)!
            )
            .ToArray();
    }

    public PostProcessorWrapper[] GetPostProcessorsFor(IEvent @event, IServiceScope scope)
    {
        Type wrapperType = typeof(PostProcessorWrapper<>).MakeGenericType(@event.GetType());
        Type processorType = typeof(IPostProcessor<>).MakeGenericType(@event.GetType());

        return scope
            .ServiceProvider.GetServices(processorType)
            .Where(x => x is not null)
            .Select(service =>
                (Activator.CreateInstance(wrapperType, service) as PostProcessorWrapper)!
            )
            .ToArray();
    }

    public bool TryGetPreAppendEventActionFor(
        IEvent @event,
        IServiceProvider serviceProvider,
        out List<PreAppendEventActionWrapper>? actions
    )
    {
        Type wrapperType = typeof(PreAppendEventActionWrapper<>).MakeGenericType(@event.GetType());
        Type actionType = typeof(IPreAppendEventAction<>).MakeGenericType(@event.GetType());

        actions = serviceProvider
            .GetServices(actionType)
            .Where(x => x is not null)
            .Select(service =>
                (Activator.CreateInstance(wrapperType, service) as PreAppendEventActionWrapper)!
            )
            .ToList();

        return actions.Count > 0;
    }

    public bool TryGetPostAppendEventActionsFor(
        IEvent @event,
        IServiceProvider serviceProvider,
        out List<PostAppendEventActionWrapper>? actions
    )
    {
        Type wrapperType = typeof(PostAppendEventActionWrapper<>).MakeGenericType(@event.GetType());
        Type actionType = typeof(IPostAppendEventAction<>).MakeGenericType(@event.GetType());

        actions = serviceProvider
            .GetServices(actionType)
            .Where(x => x is not null)
            .Select(service =>
                (Activator.CreateInstance(wrapperType, service) as PostAppendEventActionWrapper)!
            )
            .ToList();

        return actions.Count > 0;
    }
}
