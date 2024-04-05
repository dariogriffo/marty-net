namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PreProcessorWrapper
{
    internal abstract Task Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    );
}

internal class PreProcessorWrapper<T>(IPreProcessor<T> processor) : PreProcessorWrapper
    where T : IEvent
{
    internal override Task Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    ) => processor.Execute((T)@event, context, cancellationToken);
}