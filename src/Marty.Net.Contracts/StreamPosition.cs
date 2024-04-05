namespace Marty.Net.Contracts;

/// <summary>
///     A position to subscribe from in a stream
/// </summary>
public enum SubscriptionPosition
{
    /// <summary>
    ///     The start of the stream
    /// </summary>
    Start,

    /// <summary>
    ///     The end of the stream
    /// </summary>
    End
}
