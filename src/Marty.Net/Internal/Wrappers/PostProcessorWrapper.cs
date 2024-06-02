namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PostProcessorWrapper
{
    internal abstract Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken = default
    );
}

internal class PostProcessorWrapper<T> : PostProcessorWrapper
    where T : IEvent
{
    private readonly IPostProcessor<T> _processor;

    public PostProcessorWrapper(IPostProcessor<T> processor)
    {
        _processor = processor;
    }

    internal override Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken = default
    ) => _processor.Execute((T)@event, context, result, cancellationToken);
}
