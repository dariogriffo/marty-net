namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Wrappers;

internal sealed class HandlesFactory : IHandlesFactory
{
    internal record EventRegistry
    {
        internal (Type Wrapper, Type Service) Behaviors { get; init; }
        internal (Type Wrapper, Type Service) PreProcessorWrappers { get; init; }
        internal (Type Wrapper, Type Service) PostProcessorWrappers { get; init; }
        internal (Type Wrapper, Type Service) Handler { get; init; }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, EventRegistry> _plans = new();

    public HandlesFactory(
        IServiceProvider serviceProvider,
        HandlersAndEventTypes handlersAndEventTypes
    )
    {
        _serviceProvider = serviceProvider;
        foreach (Type eventType in handlersAndEventTypes.RegisteredEventsAndHandlers)
        {
            SetupEventRegistry(eventType);
        }
    }

    private void SetupEventRegistry(Type eventType)
    {
        Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
        Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        Type behaviorWrapperType = typeof(PipelineBehaviorWrapper<>).MakeGenericType(eventType);
        Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(eventType);
        Type preProcessorWrapperType = typeof(PreProcessorWrapper<>).MakeGenericType(eventType);
        Type preProcessorType = typeof(IPreProcessor<>).MakeGenericType(eventType);
        Type postWrapperType = typeof(PostProcessorWrapper<>).MakeGenericType(eventType);
        Type postProcessorType = typeof(IPostProcessor<>).MakeGenericType(eventType);
        _plans[eventType] = new EventRegistry()
        {
            Behaviors = (behaviorWrapperType, behaviorType),
            PreProcessorWrappers = (preProcessorWrapperType, preProcessorType),
            PostProcessorWrappers = (postWrapperType, postProcessorType),
            Handler = (wrapperType, handlerType),
        };
    }

    class ScopeDisposerBehavior : PipelineBehaviorWrapper
    {
        private IServiceScope _scope;

        public ScopeDisposerBehavior(IServiceScope scope)
        {
            _scope = scope;
        }

        internal override async Task<OperationResult> Execute(
            object @event,
            IConsumerContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                OperationResult result = await Next(@event, context, cancellationToken);
                return result;
            }
            finally
            {
                _scope.Dispose();
            }
        }
    }

    class ExecutorBehavior : PipelineBehaviorWrapper
    {
        private readonly EventHandlerWrapper _handler;
        private readonly PreProcessorWrapper[]? _preProcessors;
        private readonly PostProcessorWrapper[]? _postProcessors;

        public ExecutorBehavior(
            EventHandlerWrapper handler,
            PreProcessorWrapper[]? preProcessors,
            PostProcessorWrapper[]? postProcessors
        )
        {
            _handler = handler;
            _preProcessors = preProcessors;
            _postProcessors = postProcessors;
        }

        internal override async Task<OperationResult> Execute(
            object @event,
            IConsumerContext context,
            CancellationToken cancellationToken = default
        )
        {
            if (_preProcessors is not null)
            {
                foreach (PreProcessorWrapper wrapper in _preProcessors)
                {
                    await wrapper.Execute(@event, context, cancellationToken);
                }
            }

            OperationResult result = await _handler.Handle(@event, context, cancellationToken);
            if (_postProcessors is null)
            {
                return result;
            }

            foreach (PostProcessorWrapper wrapper in _postProcessors)
            {
                result = await wrapper.Execute(@event, context, result, cancellationToken);
            }

            return result;
        }
    }

    public ExecutionPlan? TryGetExecutionPlanFor(IEvent @event)
    {
        Type eventType = @event.GetType();
        if (!_plans.TryGetValue(eventType, out EventRegistry? registry))
        {
            return null;
        }

        IServiceScope scope = _serviceProvider.CreateScope();

        EventHandlerWrapper handler = CreateWrappedService<EventHandlerWrapper>(
            scope,
            (registry.Handler.Wrapper, registry.Handler.Service)
        );

        PipelineBehaviorWrapper[]? behaviorWrappers =
            CreateWrappedServices<PipelineBehaviorWrapper>(scope, registry.Behaviors);

        PreProcessorWrapper[]? preProcessorWrappers = CreateWrappedServices<PreProcessorWrapper>(
            scope,
            registry.PreProcessorWrappers
        );

        PostProcessorWrapper[]? postProcessorWrappers = CreateWrappedServices<PostProcessorWrapper>(
            scope,
            registry.PostProcessorWrappers
        );

        int behaviorsSize =
            1 //Scope disposer
            + (behaviorWrappers?.Length ?? 0)
            + //All behaviors
            +1; //Handler + processors

        PipelineBehaviorWrapper[] behaviors = new PipelineBehaviorWrapper[behaviorsSize];

        //We dispose the scope at the end of the pipeline
        behaviors[0] = new ScopeDisposerBehavior(scope);

        //last behavior executes the handlers and any registered pre and post processors
        behaviors[^1] = new ExecutorBehavior(handler, preProcessorWrappers, postProcessorWrappers);

        behaviorWrappers?.CopyTo(behaviors, 1);

        return new(behaviors);
    }

    private static T CreateWrappedService<T>(
        IServiceScope scope,
        (Type Wrapper, Type Service) tuple
    )
        where T : class
    {
        object service = scope.ServiceProvider.GetService(tuple.Service)!;
        T wrapper = (Activator.CreateInstance(tuple.Wrapper, service) as T)!;
        return wrapper;
    }

    private static T[]? CreateWrappedServices<T>(
        IServiceScope scope,
        (Type Wrapper, Type Service) tuple
    )
        where T : class
    {
        IEnumerable<object?> services = scope.ServiceProvider.GetServices(tuple.Service);
        T[] wrappers = null!;
        if (services.Any())
        {
            wrappers = services
                .Where(x => x is not null)
                .Select(service => (Activator.CreateInstance(tuple.Wrapper, service) as T)!)
                .ToArray();
        }

        return wrappers;
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
