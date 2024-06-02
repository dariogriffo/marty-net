namespace Marty.Net.Contracts;

using System;
using Aggregates.Contracts;

/// <summary>
///     Configuration for the <see cref="ServiceCollectionExtensions.AddMartyAggregates" />
/// </summary>
public class AggregatesConfiguration
{
    /// <summary>
    ///     If set, the implementation to use for the <see cref="IAggregateStreamResolver" />
    /// </summary>
    public Type? AggregateStreamResolver { get; set; }
}
