[![N|Solid](https://avatars2.githubusercontent.com/u/39886363?s=200&v=4)](https://github.com/dariogriffo/marty-net)

# Marty.Net

An extremely opinionated framework to work with Greg Young's [EventStoreDb](https://eventstore.com/).

[![NuGet Info](https://buildstats.info/nuget/Marty.Net?includePreReleases=true)](https://www.nuget.org/packages/Marty.Net/)
[![GitHub license](https://img.shields.io/github/license/dariogriffo/marty-net.svg)](https://raw.githubusercontent.com/dariogriffo/marty-net/master/LICENSE)
### Build Status
![.Net8.0](https://github.com/dariogriffo/marty-net/workflows/.NET/badge.svg?branch=main)

[![Build history](https://buildstats.info/github/chart/dariogriffo/marty-net?branch=main&includeBuildsFromPullRequest=false)](https://github.com/dariogriffo/marty-net/actions?query=branch%3Amain++)


## Table of contents

- [About](#about)
- [Getting Started](#getting-started)
- [Examples](#examples)
- [License](#license)

## About

[Marty.Net](https://www.nuget.org/packages/Marty.Net) is an opinionated application framework for [EventStoreDb](https://eventstore.com/).

After having been working with EventStore for some time now, and finding different issues on the code using it, starting from the Aggregate down to connection management, Marty.Net was born as an effort to allow me start new projects quickly on top of EventStore.

The motivation behind it is to allow other developers of different levels to use EventStore with the minimum of effort, making simple to configure the behavior, and allow unit testing of simple things (routing, writing and reading).

Also the framework is split in 2 nuget packages to allow easy unit testing without installing all the EventStore dependencies in domain projects.

***Marty.Net*** is not meant to evolve to a heavy bloated library, but to keep things simple and working. Is focused on microservices architecture where [DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) is not much of an issue.

***Marty.Net*** main purpose is to allow you to work with EventStore in a few lines, and that's how it will always be.

### Who is it for?

[Marty.Net](https://www.nuget.org/packages/Marty.Net) is intended for developers who want to work with [event sourcing](https://www.eventstore.com/blog/what-is-event-sourcing) and a reliable event store, simplifying their code and life.

It is not designed to deal with every case, but the simple ones, the ones you will be doing 99% of the time.
It `enforces` the implementation of some interfaces with the aim of having a consistent development experience, so nobody has to worry about the basics of your event store.

## Getting Started

### With Contracts implementation
`Install-Package Marty.Net.Contracts`

- Define your events and make them implement [IEvent](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IEvent.cs#L8).
- Implement an [`IEventHandler`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IEventHandler.cs#L11)
- Implement the [`IEventsStreamResolver`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IEventsStreamResolver.cs#L6) interface to allow the [`IEventStore`](44#L81) know where to append your events.

### Goodies
- Implement an [`IPipelineBehavior`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IPipelineBehavior.cs#L13) if you want a pipeline behavior for your handlers.
- Implement an [`IPreProcessor`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IPreProcessor.cs#L12) if you to execute actions before your handlers.
- Implement an [`IPostProcessor`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IPostProcessor.cs#L12) if you to execute actions after your handlers.


That's it you can start coding, and unit testing... Now you want to see if against a real EventStore instance?

### Integrating with a real instance
`Install-Package Marty.Net`

- Add a configuration section called `Marty.Net` and configure the EventStore with the settings class [`EventStoreSettings`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/EventStoreSettings.cs#L10) with the only required property `ConnectionString`
- Add Marty.Net to your ServiceCollection (in your writer service and in your reader service) via the extension method [`services.AddMarty.Net(...)`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net/ServiceCollectionExtensions.cs#L25)
- Subscribe to a stream or projection on the EventStore with [`SubscribeToStream(...)`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IEventStore.cs#L92)
- Append your events to the EventStore with [`Append(..)`](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IEventStore.cs#L22)

That's it, the simplest way to start event sourcing

## Loading aggregates

`Install-Package Marty.Net.Aggreagtes.Contracts`
- Declare a class that inherits from [Aggregate](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Aggregates.Contracts/Aggregate.cs#L14)
- Implement privately methods with the signature `private void Apply(MyEvent1 @event)` or `private void Apply(MyEvent2 @event)`
- Implement in your StreamResolver `public string StreamForAggregate<T>(System.Guid id) where T : Aggregate`
- Load your aggregate from EventStore ` var user = await _eventStore.Get<User>(id, cancellationToken);`


Now when is time to integrate with a real instance of EventStore, just install the package

`Marty.Net.Aggregates`

and you are ready to go.

## Examples

A Publisher and Subscriber can be found [here](https://github.com/dariogriffo/marty-net/tree/main/examples)
You will find how to integrate Pipelines, how to publish events, and how to do simple event sourcing loading aggregates from EventStore.

## Retries

By default Marty.Net will not retry anything, but there is a mechanism that can be configured or even better replaced with your own retry mechanism.
To configure the out of the box retry mechanism, 3 options can be set to retry on subscriptions, reads and write on the settings, the interval en attempts for all:
you want something more powerful, like using [Polly](https://github.com/App-vNext/Polly) just implement [this interface](https://github.com/dariogriffo/marty-net/blob/main/src/Marty.Net.Contracts/IConnectionStrategy.cs) and that's it.

## License

[MIT](https://github.com/dariogriffo/marty-net/blob/main/LICENSE)
