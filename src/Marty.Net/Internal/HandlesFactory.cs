namespace Marty.Net.Internal;

using Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    private readonly HashSet<Type> _registeredHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, EventRegistry> _plans = new();

    public HandlesFactory(
        IServiceProvider serviceProvider,
        HandlersAndEventTypes handlersAndEventTypes
    )
    {
        _serviceProvider = serviceProvider;
        _registeredHandlers = handlersAndEventTypes.RegisteredEventsAndHandlers;
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
                OperationResult result = await Next();
                return result;
            }
            finally
            {
                _scope.Dispose();
            }
        }
    }

    class ProcessorsExecutorBehavior : PipelineBehaviorWrapper
    {
        private readonly PreProcessorWrapper[]? _preProcessors;
        private readonly PostProcessorWrapper[]? _postProcessors;

        public ProcessorsExecutorBehavior(
            PreProcessorWrapper[]? preProcessors,
            PostProcessorWrapper[]? postProcessors
        )
        {
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

            OperationResult result = await Next();
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
        if (!_registeredHandlers.TryGetValue(eventType, out _))
        {
            return null;
        }

        IServiceScope scope = _serviceProvider.CreateScope()!;
        EventRegistry registry = _plans.GetOrAdd(
            eventType,
            (_) =>
            {
                Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(@event.GetType());
                Type handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
                Type behaviorWrapperType = typeof(PipelineBehaviorWrapper<>).MakeGenericType(
                    @event.GetType()
                );
                Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(@event.GetType());
                Type preProcessorWrapperType = typeof(PreProcessorWrapper<>).MakeGenericType(
                    @event.GetType()
                );
                Type preProcessorType = typeof(IPreProcessor<>).MakeGenericType(@event.GetType());
                Type postWrapperType = typeof(PostProcessorWrapper<>).MakeGenericType(
                    @event.GetType()
                );
                Type postProcessorType = typeof(IPostProcessor<>).MakeGenericType(@event.GetType());
                return new EventRegistry()
                {
                    Behaviors = (behaviorWrapperType, behaviorType),
                    PreProcessorWrappers = (preProcessorWrapperType, preProcessorType),
                    PostProcessorWrappers = (postWrapperType, postProcessorType),
                    Handler = (wrapperType, handlerType),
                };
            }
        );

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
            +((preProcessorWrappers is not null || postProcessorWrappers is not null) ? 1 : 0); //Handler + processors

        PipelineBehaviorWrapper[] behaviors = new PipelineBehaviorWrapper[behaviorsSize];

        behaviors[0] = new ScopeDisposerBehavior(scope);

        if (preProcessorWrappers is not null || postProcessorWrappers is not null)
        {
            behaviors[^1] = new ProcessorsExecutorBehavior(
                preProcessorWrappers,
                postProcessorWrappers
            );
        }

        behaviorWrappers?.CopyTo(behaviors, 1);

        return new(handler, behaviors);
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
        T[]? wrappers = null!;
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
