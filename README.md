# TinyBlueWhale.MiniBus

Minimal high-performance in-process mediator for .NET.

Focused only on what most applications actually use every day:

* `Send()`
* `Publish()`

No pipelines.
No behaviors.
No decorators.
No framework ceremony.

Built for:

* Minimal APIs
* CQRS-lite architectures
* Vertical Slice Architecture
* Fast startup
* Low allocations
* Simple codebases

---

# Why?

Most projects use MediatR for:

```csharp
await mediator.Send(command);
await mediator.Publish(@event);
```

That is the real-world 80%.

TinyBlueWhale.MiniBus exists to solve exactly that with:

* fewer allocations
* faster startup
* simpler internals
* zero unnecessary abstractions
* extremely small codebase

---

# Features

* Minimal API surface
* Assembly scanning
* Request/Response support
* Multi-handler event publishing
* Native Dependency Injection support
* CancellationToken propagation
* Fast startup time
* Low memory allocations
* Zero external mediator dependencies

---

# Installation

```bash
Install-Package TinyBlueWhale.MiniBus
```

or

```bash
dotnet add package TinyBlueWhale.MiniBus
```

---

# Quick Start

## 1. Register MiniBus

```csharp
using TinyBlueWhale.MiniBus;

builder.Services.AddMiniBus(typeof(CreateOrderHandler).Assembly);
```

---

## 2. Create a Request

```csharp
public sealed record CreateOrderCommand(
    string CustomerEmail,
    decimal TotalAmount)
    : IRequest<Guid>;
```

---

## 3. Create a Handler

```csharp
public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
```

---

## 4. Send Request

```csharp
var orderId = await miniBus.Send(
    new CreateOrderCommand(
        "customer@company.com",
        1499.99m));
```

---

# Events

## Create Event

```csharp
public sealed record OrderCreatedEvent(
    Guid OrderId,
    string CustomerEmail);
```

---

## Create Event Handler

```csharp
public sealed class SendConfirmationEmailHandler
    : IEventHandler<OrderCreatedEvent>
{
    public Task Handle(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Confirmation email sent to '{@event.CustomerEmail}'.");

        return Task.CompletedTask;
    }
}
```

---

## Publish Event

```csharp
await miniBus.Publish(
    new OrderCreatedEvent(orderId, customerEmail));
```

---

# Benchmarks

Environment:

* .NET 9
* Intel i7-1185G7
* BenchmarkDotNet

---

## Startup Registration

| Method                |       Mean | Allocated |
| --------------------- | ---------: | --------: |
| TinyBlueWhale.MiniBus |   7.067 ms |   2.63 MB |
| MediatR               | 565.032 ms |  23.56 MB |

TinyBlueWhale.MiniBus startup registration is ~80x faster.

---

## Send()

| Method                |     Mean | Allocated |
| --------------------- | -------: | --------: |
| TinyBlueWhale.MiniBus | 54.40 ns |     168 B |
| MediatR               | 87.32 ns |     336 B |

TinyBlueWhale.MiniBus executes requests ~1.6x faster with half the allocations.

---

## Publish() with 50 Handlers

| Method                |     Mean | Allocated |
| --------------------- | -------: | --------: |
| TinyBlueWhale.MiniBus | 1.565 μs |   1.17 KB |
| MediatR               | 1.544 μs |   7.55 KB |

TinyBlueWhale.MiniBus keeps similar publish throughput with significantly lower memory allocations.

---

# Example Project

Repository includes a complete Minimal API example:

```text
TinyBlueWhale.MiniBus.Example.ECommerce
```

Includes:

* Commands
* Queries
* Events
* Multiple event handlers
* Swagger
* Vertical Slice Architecture

---

# Philosophy

TinyBlueWhale.MiniBus intentionally avoids:

* pipelines
* behaviors
* middleware chains
* decorators
* feature creep
* enterprise ceremony

If your application needs advanced orchestration frameworks, MediatR is still an excellent choice.

TinyBlueWhale.MiniBus is designed for applications that want:

* simplicity
* speed
* maintainability
* tiny abstractions

---

# Roadmap

Potential future improvements:

* source generators
* compile-time registration
* analyzers
* AOT optimizations

Without sacrificing simplicity.

---

# Contributing

Contributions are welcome.

Rules:

* Keep the core minimal.
* Avoid unnecessary abstractions.
* Performance regressions are not accepted.
* Every feature must justify its complexity.

