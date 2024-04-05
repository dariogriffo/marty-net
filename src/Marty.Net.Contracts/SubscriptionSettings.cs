namespace Marty.Net.Contracts;

using System;

/// <summary>
///     The configuration for a Subscription
/// </summary>
public class SubscriptionSettings
{
    /// <summary>
    ///     The group for the persistent subscription.
    ///     Required if processing with event handlers with Persistent subscription
    /// </summary>
    public string SubscriptionGroup { get; set; } = null!;

    /// <summary>
    /// </summary>
    public bool ResolveLinkTos { get; set; } = false;

    /// <summary>
    /// </summary>
    public bool ExtraStatistics { get; set; } = false;

    /// <summary>
    /// </summary>
    public TimeSpan? MessageTimeout { get; set; } = null;

    /// <summary>
    /// </summary>
    public int MaxRetryCount { get; set; } = 10;

    /// <summary>
    /// </summary>
    public int LiveBufferSize { get; set; } = 500;

    /// <summary>
    /// </summary>
    public int ReadBatchSize { get; set; } = 20;

    /// <summary>
    /// </summary>
    public int HistoryBufferSize { get; set; } = 500;

    /// <summary>
    /// </summary>
    public TimeSpan? CheckPointAfter { get; set; } = null;

    /// <summary>
    /// </summary>
    public int CheckPointLowerBound { get; set; } = 10;

    /// <summary>
    /// </summary>
    public int CheckPointUpperBound { get; set; } = 1000;

    /// <summary>
    /// </summary>
    public int MaxSubscriberCount { get; set; } = 0;

    /// <summary>
    /// </summary>
    public string ConsumerStrategyName { get; set; } = "RoundRobin";
}
