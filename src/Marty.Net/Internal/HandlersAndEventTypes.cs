namespace Marty.Net.Internal;

using Contracts.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

internal sealed class HandlersAndEventTypes
{
    private readonly Lazy<HashSet<Type>> _handlers;

    internal HandlersAndEventTypes(IServiceCollection services)
    {
        _handlers = services.GetTypesFor(typeof(EventHandlerAttribute));
    }

    internal HashSet<Type> RegisteredEventsAndHandlers => _handlers.Value;
}
