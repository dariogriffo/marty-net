namespace Marty.Net.Contracts.Internal;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Inspired in https://github.com/jbogard/MediatR
/// License included
/// </summary>

internal static class ServiceRegistrar
{
    private static readonly string InterfacesNames = typeof(IEvent).Namespace!;

    internal static IEnumerable<Type> FindInterfacesThatClose(
        this Type pluggedType,
        Type templateType
    )
    {
        return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
    }

    private static IEnumerable<Type> FindInterfacesThatClosesCore(
        Type? pluggedType,
        Type templateType
    )
    {
        if (pluggedType is null)
        {
            yield break;
        }

        if (!pluggedType.IsConcrete())
        {
            yield break;
        }

        if (templateType.IsInterface)
        {
            foreach (
                Type interfaceType in pluggedType
                    .GetInterfaces()
                    .Where(type =>
                        type.IsGenericType && (type.GetGenericTypeDefinition() == templateType)
                    )
            )
            {
                yield return interfaceType;
            }
        }
        else if (
            pluggedType.BaseType!.IsGenericType
            && (pluggedType.BaseType!.GetGenericTypeDefinition() == templateType)
        )
        {
            yield return pluggedType.BaseType!;
        }

        if (pluggedType.BaseType == typeof(object))
            yield break;

        foreach (
            Type interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType)
        )
        {
            yield return interfaceType;
        }
    }

    private static bool IsConcrete(this Type type)
    {
        return type is { IsAbstract: false, IsInterface: false };
    }

    private static bool Implements(this Type type, Type attributeType)
    {
        HashSet<Type> interfaces = type.GetInterfaces()
            .Where(x => x.Namespace == InterfacesNames)
            .ToHashSet();
        return interfaces.Any(i =>
            i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType)
        );
    }

    internal static Lazy<HashSet<Type>> GetTypesFor(this IServiceCollection services, Type type)
    {
        return new Lazy<HashSet<Type>>(
            () =>
                services
                    .Where(x =>
                        x.ImplementationType is not null
                        && !x.ImplementationType!.IsAbstract
                        && x.ImplementationType.Implements(type)
                    )
                    .Select(x => x.ServiceType.GenericTypeArguments.First())
                    .ToHashSet()
        );
    }
}
