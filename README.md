# Rebus RabbitMQ Example

A minimal end-to-end sample that shows how to use **[Rebus](https://github.com/rebus-org/Rebus)** with **RabbitMQ**.

The solution contains:

- `Rebus.RabbitMq.Publisher`: a CLI app that publishes/sends messages to RabbitMQ using Rebus.
- `Rebus.RabbitMq.Consumer`: a hosted service that subscribes to messages and handles them.
- `Rebus.RabbitMq.Common`: shared message contracts.

The sample is intentionally small and uses an in-memory configuration for the RabbitMQ connection string.

## Prerequisites

- .NET SDK (the projects target modern .NET; use the SDK version configured in the repo)
- A running RabbitMQ broker
  - Default local endpoint assumed by this sample: `amqp://guest:guest@localhost:5672/TEST`

### Run RabbitMQ locally (Docker)

```bash
docker run --rm -it \
  -p 5672:5672 \
  -p 15672:15672 \
  --name rabbitmq \
  rabbitmq:3-management
```

RabbitMQ management UI: http://localhost:15672 (default credentials: `guest` / `guest`).

## Solution structure

- `Rebus.RabbitMq.Common`
  - `SampleEvent`
  - `SampleDelayEvent`
  - `CriticalEvent`

- `Rebus.RabbitMq.Consumer`
  - Configures Rebus with RabbitMQ transport and subscribes to the messages at startup
  - Registers handlers:
    - `SampleEventConsumer`
    - `SampleDelayEventConsumer`
    - `CriticalEventConsumer`

- `Rebus.RabbitMq.Publisher`
  - Exposes a small CLI (based on `System.CommandLine`) with commands:
    - `publish-sample-event`
    - `publish-sample-delay-event`
    - `publish-critical-event`

## How it works

### Transport

- Publisher uses RabbitMQ as a **one-way client** (`UseRabbitMqAsOneWayClient`).
- Consumer uses RabbitMQ with an **input queue** named `rebus-rabbitmq`.

### Routing

This sample uses type-based routing:

- `SampleEvent` and `SampleDelayEvent` are **published**.
- `CriticalEvent` is **sent** directly.

Notes based on current code:

- Publisher maps `CriticalEvent` to `critical-event-queue`.
- Consumer maps `CriticalEvent` to `critical-event-exchange` and also subscribes to it.

If you want `CriticalEvent` to be purely point-to-point, align both sides to use the same queue name and prefer `Send` + `IHandleMessages<CriticalEvent>` without subscribing.

### Retries

The consumer configures a simple retry strategy with:

- `maxDeliveryAttempts: 5`

`SampleDelayEventConsumer` intentionally throws an exception to demonstrate retries.

### Logging

Both apps configure **NLog** programmatically:

- Console logging
- File logging to `logs/app.log` with rolling archives in `logs/archives/`

## Build

From the repository root:

```bash
dotnet build
```

## Run the consumer

From the repository root:

```bash
dotnet run --project Rebus.RabbitMq.Consumer
```

The consumer will start, configure Rebus, subscribe to the message types, and wait for messages.

## Run the publisher

From the repository root:

```bash
dotnet run --project Rebus.RabbitMq.Publisher -- publish-sample-event -num 10
```

### Available commands

#### Publish `SampleEvent`

```bash
dotnet run --project Rebus.RabbitMq.Publisher -- publish-sample-event -num 50
```

#### Publish `SampleDelayEvent` (will trigger retries on the consumer)

```bash
dotnet run --project Rebus.RabbitMq.Publisher -- publish-sample-delay-event -num 5
```

#### Send `CriticalEvent`

```bash
dotnet run --project Rebus.RabbitMq.Publisher -- publish-critical-event -num 3
```

## Configuration

RabbitMQ connection is currently set in code via an in-memory settings dictionary:

- `RabbitMQ:ConnectionString` = `amqp://guest:guest@localhost:5672/TEST`

If you want to make it configurable via `appsettings.json` or environment variables, move the setting into configuration providers and read it from `context.Configuration`.

## Troubleshooting

- **Nothing is being received**
  - Ensure RabbitMQ is running and reachable.
  - Ensure you started the consumer before publishing.
  - Check that routing for `CriticalEvent` matches between publisher and consumer.

- **Consumer keeps retrying**
  - This is expected for `SampleDelayEvent`: `SampleDelayEventConsumer` throws by design.

- **Check logs**
  - Console output shows message flow.
  - File logs are written under `logs/` next to the application binaries.

## License

Add your license information here (or refer to the repository license file if present).
