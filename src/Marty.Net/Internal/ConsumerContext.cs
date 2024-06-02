namespace Marty.Net.Internal;

using System.Collections.Generic;
using Contracts;

internal sealed class ConsumerContext : IConsumerContext
{
    internal ConsumerContext(
        string streamName,
        IReadOnlyDictionary<string, string>? metadata = null,
        int? retryCount = null
    )
    {
        StreamName = streamName;
        Metadata = metadata;
        RetryCount = retryCount;
    }

    public int? RetryCount { get; }

    public string StreamName { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; }
}
