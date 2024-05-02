namespace Marty.Net;

using Contracts;
using Contracts.Internal;
using Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
///     The integration point with the <see cref="Microsoft.Extensions.DependencyInjection" /> framework
/// </summary>
public static class ServiceCollectionExtensions
{
    private static HandlersAndEventTypes? _handlersAndTypes;

    /// <summary>
    ///     Adds Marty.Net to your app allowing to configuring options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the registration happens.</param>
    /// <param name="actionsConfigurator">The configuration for the Dependency Injection.</param>
    /// <param name="eventStoreSettingsProvider">A function to resolve the <see cref="EventStoreSettings" />.</param>
    /// <exception cref="ArgumentException">
    ///     The <see cref="IEventsStreamResolver" /> type provided does not implement the
    ///     <see cref="IEventsStreamResolver" /> interface.
    /// </exception>
    /// <returns>.</returns>
    public static IServiceCollection AddMarty(
        this IServiceCollection services,
        Func<IServiceProvider, EventStoreSettings>? eventStoreSettingsProvider,
        Action<MartyConfiguration>? actionsConfigurator = default
    )
    {
        static void ValidateProvidedType(Type type, Type expectedInterfaceType, string s)
        {
            if (type.GetInterfaces().All(x => x != expectedInterfaceType))
            {
                throw new ArgumentException(s, nameof(type));
            }
        }

        Action<MartyConfiguration> decoratorConfigurator = config =>
        {
            actionsConfigurator?.Invoke(config);
            config.EventsStreamResolverType ??= typeof(NoEventsStreamResolver);
            config.ReconnectionStrategyType ??= typeof(NoReconnectionStrategy);
        };

        MartyConfiguration configuration = new();
        decoratorConfigurator.Invoke(configuration);

        Type eventsStreamResolverType = configuration.EventsStreamResolverType!;
        Type reconnectionStrategyType = configuration.ReconnectionStrategyType!;

        const string messageTemplate = "The {0} type doesn't implement the interface: {1}.";

        Type expectedInterfaceType = typeof(IEventsStreamResolver);
        Type providedType = eventsStreamResolverType;
        string message = string.Format(messageTemplate, providedType, expectedInterfaceType);
        ValidateProvidedType(providedType, expectedInterfaceType, message);

        expectedInterfaceType = typeof(IConnectionStrategy);
        providedType = reconnectionStrategyType;
        message = string.Format(messageTemplate, providedType, expectedInterfaceType);
        ValidateProvidedType(providedType, expectedInterfaceType, message);

        services.AddSingleton(configuration.ReconnectionStrategyType!);
        services.AddSingleton(configuration.EventsStreamResolverType!);

        services.AddTransient<IConnectionStrategy>(sp =>
            (IConnectionStrategy)
                sp.GetRequiredService(
                    sp.GetRequiredService<
                        IOptions<MartyConfiguration>
                    >().Value.ReconnectionStrategyType!
                )
        );

        services.AddSingleton<IEventsStreamResolver>(sp =>
            (IEventsStreamResolver)
                sp.GetRequiredService(
                    sp.GetRequiredService<
                        IOptions<MartyConfiguration>
                    >().Value.EventsStreamResolverType!
                )
        );

        services.AddSingleton(sp =>
        {
            EventStoreSettings eventStoreSettings = eventStoreSettingsProvider!.Invoke(sp);
            _ =
                eventStoreSettings.ConnectionString
                ?? throw new ArgumentNullException(nameof(eventStoreSettings.ConnectionString));
            return eventStoreSettings;
        });

        services.AddSingleton<ISerializer, Serializer>();
        services.AddSingleton<IHandlesFactory, HandlesFactory>();
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IReadEventStore, ReadEventStore>();
        services.AddScoped<IWriteEventStore, WriteEventStore>();
        services.AddScoped<IEventStore, EventStore>();
        services.AddSingleton<IPersistentSubscriber, PersistentSubscriber>();
        services.AddSingleton<IInternalPersistentSubscriber, InternalPersistentSubscriber>();
        services.AddSingleton<IConnectionProvider, ConnectionProvider>();
        services.TryAddSingleton(services);
        services.Configure(decoratorConfigurator);

        DiscoverAndRegisterHandlers(services, configuration);
        PopulatePipelinesAndActions(services, configuration);

        _handlersAndTypes = new HandlersAndEventTypes(services);
        services.AddSingleton(_handlersAndTypes);

        return services;
    }

    private static void PopulatePipelinesAndActions(
        IServiceCollection services,
        MartyConfiguration configuration
    )
    {
        configuration.PipelineBehaviorsToRegister.ForEach(services.Add);
        configuration.PreAppendersToRegister.ForEach(services.Add);
        configuration.PostAppendersToRegister.ForEach(services.Add);
        configuration.PreProcessorsToRegister.ForEach(services.Add);
        configuration.PostProcessorsToRegister.ForEach(services.Add);
    }

    private static void DiscoverAndRegisterHandlers(
        IServiceCollection services,
        MartyConfiguration configuration
    )
    {
        Assembly[]? handlersAssemblies = configuration.AssembliesToScanForHandlers;
        if (handlersAssemblies is null || handlersAssemblies.Length <= 0)
        {
            return;
        }

        RegisterTargetWithAttribute(
            services,
            handlersAssemblies,
            typeof(EventHandlerAttribute),
            configuration.DefaultHandlesLifetime
        );
    }

    private static void RegisterTargetWithAttribute(
        IServiceCollection services,
        Assembly[] assemblies,
        Type attributeType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        IEnumerable<Type> handlers = assemblies
            .Distinct()
            .SelectMany(assembly =>
                GetNonAbstractClasses(assembly)
                    .Where(c => ImplementsAnInterfaceDecoratedWithAttribute(c, attributeType))
            );

        foreach (Type handler in handlers)
        {
            foreach (
                Type target in handler
                    .GetInterfaces()
                    .Where(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType))
            )
            {
                ServiceDescriptor descriptor = new(target, handler, lifetime);
                services.Add(descriptor);
            }
        }

        return;

        static IEnumerable<Type> GetNonAbstractClasses(Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(y => !y.IsAbstract);
        }

        static bool ImplementsAnInterfaceDecoratedWithAttribute(Type type, Type attribute)
        {
            return type.GetInterfaces()
                .Any(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attribute));
        }
    }
}
