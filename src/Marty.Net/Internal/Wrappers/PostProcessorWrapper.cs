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

internal class PostProcessorWrapper<T>(IPostProcessor<T> processor) : PostProcessorWrapper
    where T : IEvent
{
    internal override Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken = default
    ) => processor.Execute((T)@event, context, result, cancellationToken);
}
