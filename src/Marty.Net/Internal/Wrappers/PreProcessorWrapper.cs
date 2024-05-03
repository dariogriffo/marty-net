namespace Marty.Net.Internal.Wrappers;

using Contracts;
using System.Threading;
using System.Threading.Tasks;

internal abstract class PreProcessorWrapper
{
    internal abstract Task Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    );
}

internal class PreProcessorWrapper<T> : PreProcessorWrapper
    where T : IEvent
{
    private readonly IPreProcessor<T> _processor;

    public PreProcessorWrapper(IPreProcessor<T> processor)
    {
        _processor = processor;
    }

    internal override Task Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    ) => _processor.Execute((T)@event, context, cancellationToken);
}
