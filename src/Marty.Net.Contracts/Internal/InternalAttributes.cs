namespace Marty.Net.Contracts.Internal;

using System;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class EventHandlerAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PreProcessorEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class EventPostProcessorAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PipelineBehaviorAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PreAppendEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class AfterPublishEventAttribute : Attribute;
