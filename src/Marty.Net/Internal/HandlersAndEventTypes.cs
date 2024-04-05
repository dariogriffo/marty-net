namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using Contracts.Internal;
using Microsoft.Extensions.DependencyInjection;

internal sealed class HandlersAndEventTypes
{
    private readonly Lazy<HashSet<Type>> _handlers;

    internal HandlersAndEventTypes(IServiceCollection services)
    {
        _handlers = services.GetTypesFor(typeof(EventHandlerAttribute));
    }

    internal HashSet<Type> RegisteredEventsAndHandlers => _handlers.Value;
}
