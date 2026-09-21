# TinyBlueWhale.MiniBus

Minimal in-process mediator for .NET.

MiniBus provides request/response messaging and event publishing through a small, explicit API designed for applications that need mediator-style dispatch without introducing unnecessary infrastructure.

Use commands, queries and events while keeping handlers independent from their callers and integrated with Microsoft Dependency Injection.

> MiniBus 1.0.0 is the current stable release.

---

# Contents

* The Problem
* Why MiniBus Exists
* Philosophy
* Design Principles
* When Should I Use MiniBus?
* Features
* Architecture
* Installation
* Quick Start
* Requests
* Events
* Dependency Injection
* ECommerce Example
* Benchmarks
* Testing Strategy
* FAQ
* Project Status
* Documentation
* Support
* License

---

# The Problem

Application components frequently need to execute business operations without becoming directly coupled to their implementations.

A controller, endpoint or application service may need to create an order, retrieve data or notify multiple components that something happened.

Direct dependencies work well for simple applications, but as the application grows they can introduce unnecessary coupling between callers and business operations.

Mediator-style messaging provides a simple alternative.

The caller describes what should happen through a request or event while the corresponding handlers contain the behavior.

---

# Why MiniBus Exists

MiniBus was designed around one simple idea:

> In-process messaging should remain simple and explicit.

MiniBus focuses on two operations:

* `Send` for request/response messaging.
* `Publish` for event notification.

It intentionally avoids introducing transport infrastructure, persistence, distributed messaging or additional abstractions that are unrelated to in-process dispatch.

The objective is not to hide application behavior.

The objective is to decouple callers from handlers while keeping the execution model easy to understand.

---

# Philosophy

MiniBus follows a small set of engineering principles that influence every design decision.

## Explicit over Implicit

Handlers are explicitly defined through request and event contracts.

Assemblies containing handlers are explicitly registered during application startup.

---

## Small Public API

MiniBus exposes only the abstractions required for request and event dispatch.

The library does not attempt to become an application framework.

---

## Focused Infrastructure

MiniBus focuses exclusively on in-process messaging.

Responsibilities intentionally outside its scope include:

* Cross-process messaging
* Message brokers
* Message persistence
* Retries
* Distributed transactions
* Scheduling
* Workflow orchestration

---

## Composition over Framework Behavior

MiniBus integrates with the standard .NET Dependency Injection container instead of introducing its own dependency model.

Handlers can use normal constructor injection and participate naturally in an application's existing dependency graph.

---

# Design Principles

MiniBus was built around a small set of engineering principles.

* Small and explicit public API
* Request/response messaging
* Event publishing
* Assembly-based handler discovery
* Standard .NET Dependency Injection
* Explicit assembly registration
* Cancellation token propagation
* Predictable dispatch behavior
* Minimal infrastructure

---

# When Should I Use MiniBus?

MiniBus is useful when an application needs to separate callers from business operation handlers while keeping all messaging inside the same process.

Typical scenarios include:

* Minimal APIs
* Web APIs
* CQRS-style applications
* Modular monoliths
* Application-layer commands
* Application-layer queries
* Domain or application events
* Background services

MiniBus is not a replacement for a message broker when messages must cross process or machine boundaries.

---

# Features

MiniBus provides a focused in-process messaging model.

## Requests

* Request/response dispatch
* One handler per request type
* Generic response types
* Cancellation token propagation
* Missing-handler validation
* Duplicate-handler validation

## Events

* Zero or multiple handlers per event
* Sequential handler execution
* Cancellation token propagation
* Exception propagation

## Infrastructure

* Assembly-based handler discovery
* Multiple assembly registration
* Multiple `AddMiniBus` registrations
* Idempotent assembly registration
* Microsoft Dependency Injection integration
* .NET 9 and .NET 10 support
* Automated tests
* Benchmark project
* Executable ECommerce example

---

# Architecture

MiniBus separates handler discovery, registration and runtime dispatch.

```text
Application
    │
    ▼
 IMiniBus
    │
    ├── Send(request)
    │       │
    │       ▼
    │   Request Handler
    │       │
    │       ▼
    │    Response
    │
    └── Publish(event)
            │
            ├── Event Handler
            ├── Event Handler
            └── Event Handler
```

Handler discovery occurs during Dependency Injection registration.

At runtime, MiniBus uses the resulting registry to locate the appropriate request or event handlers.

This keeps reflection-based discovery outside the runtime dispatch path.

---

## Request Dispatch

Every request follows the same dispatch flow.

```text
IRequest<TResponse>
        │
        ▼
     IMiniBus
        │
        ▼
MiniBus Registry
        │
        ▼
Request Handler Wrapper
        │
        ▼
IRequestHandler<TRequest, TResponse>
        │
        ▼
    TResponse
```

Each request type must have exactly one registered request handler.

---

## Event Dispatch

Events may have zero or multiple handlers.

```text
Event
  │
  ▼
IMiniBus
  │
  ▼
MiniBus Registry
  │
  ├── Event Handler Wrapper
  ├── Event Handler Wrapper
  └── Event Handler Wrapper
```

Event handlers are executed sequentially.

Publishing an event with no registered handlers completes successfully.

---

# Installation

Install the MiniBus package.

```bash
dotnet add package TinyBlueWhale.MiniBus
```

MiniBus targets:

* .NET 9
* .NET 10

---

# Quick Start

Register MiniBus and provide the assemblies containing your handlers.

```csharp
using TinyBlueWhale.MiniBus.DependencyInjection;

builder.Services.AddMiniBus(typeof(Program).Assembly);
```

Inject `IMiniBus`.

```csharp
public sealed class OrderService(IMiniBus miniBus)
{
    public Task<Guid> CreateOrder(string customerEmail, decimal totalAmount, CancellationToken cancellationToken)
    {
        return miniBus.Send(
            new CreateOrderCommand(customerEmail, totalAmount),
            cancellationToken);
    }
}
```

---

# Requests

Define a request.

```csharp
public sealed record CreateOrderCommand(
    string CustomerEmail,
    decimal TotalAmount) : IRequest<Guid>;
```

Implement its handler.

```csharp
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    public Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();

        return Task.FromResult(orderId);
    }
}
```

Dispatch the request.

```csharp
var orderId = await miniBus.Send(
    new CreateOrderCommand("customer@example.com", 125.00m),
    cancellationToken);
```

Each request type must have exactly one registered handler.

---

# Events

Define an event.

```csharp
public sealed record OrderCreatedEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount);
```

Implement one or more handlers.

```csharp
public sealed class SendConfirmationEmailHandler : IEventHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Confirmation email sent to '{@event.CustomerEmail}'.");

        return Task.CompletedTask;
    }
}
```

Publish the event.

```csharp
await miniBus.Publish(
    new OrderCreatedEvent(orderId, customerEmail, totalAmount),
    cancellationToken);
```

Events may have zero or multiple handlers.

Handlers are executed sequentially.

---

# Dependency Injection

MiniBus integrates with `Microsoft.Extensions.DependencyInjection`.

Register one or more assemblies:

```csharp
builder.Services.AddMiniBus(
    typeof(CreateOrderHandler).Assembly,
    typeof(AnotherHandler).Assembly);
```

Additional registrations can also be made independently:

```csharp
builder.Services.AddMiniBus(typeof(CreateOrderHandler).Assembly);
builder.Services.AddMiniBus(typeof(AnotherHandler).Assembly);
```

Repeated registration of the same assembly is ignored.

MiniBus requires at least one assembly to be explicitly provided.

---

# ECommerce Example

The repository includes an executable ECommerce example built with ASP.NET Core Minimal APIs.

The example demonstrates:

* Command dispatch
* Query dispatch
* Request/response messaging
* Event publishing
* Multiple event handlers
* Dependency Injection
* Cancellation token propagation
* Swagger integration

The order creation flow publishes an `OrderCreatedEvent` after the order has been created.

Multiple event handlers then react independently to the event.

```text
POST /orders
      │
      ▼
CreateOrderCommand
      │
      ▼
CreateOrderHandler
      │
      ├── Store Order
      │
      └── Publish OrderCreatedEvent
                    │
                    ├── SendConfirmationEmailHandler
                    ├── TrackAnalyticsHandler
                    └── UpdateInventoryHandler
```

Orders can subsequently be retrieved through a query:

```text
GET /orders/{orderId}
      │
      ▼
GetOrderQuery
      │
      ▼
GetOrderHandler
      │
      ▼
Order
```

---

# Benchmarks

MiniBus includes a BenchmarkDotNet project for evaluating dispatch performance.

Benchmarks focus on MiniBus dispatch behavior rather than application handler execution or external infrastructure.

Performance claims should be evaluated against the benchmark results for the version being used.

---

# Testing Strategy

MiniBus validates its public behavior through automated tests.

Current validation includes:

* Request dispatch
* Response handling
* Event publishing
* Multiple event handlers
* Sequential event execution
* Missing request handlers
* Duplicate request handlers
* Multiple assembly discovery
* Multiple registration calls
* Repeated assembly registration
* Dependency Injection lifetimes
* Cancellation token propagation
* Invalid registration arguments

Tests run against both .NET 9 and .NET 10.

---

# FAQ

## Is MiniBus a message broker?

No.

MiniBus provides in-process messaging only.

It does not transport messages between processes, machines or services.

---

## Is MiniBus a CQRS framework?

No.

MiniBus provides messaging primitives that work naturally with CQRS-style architectures, but it does not impose an application architecture.

---

## Can a request have multiple handlers?

No.

Each request type must resolve to exactly one request handler.

Multiple handlers for the same request are rejected during registration.

---

## Can an event have multiple handlers?

Yes.

An event may have zero or multiple handlers.

Registered handlers are executed sequentially.

---

## What happens when an event has no handlers?

Publishing completes successfully.

Events do not require a registered handler.

---

## Does MiniBus support cancellation?

Yes.

`Send` and `Publish` accept a `CancellationToken`, which is propagated to handlers.

---

## Does MiniBus support distributed messaging?

No.

Use dedicated messaging infrastructure when messages need persistence, delivery guarantees or communication across process boundaries.

---

# Project Status

MiniBus 1.0.0 is the current stable release.

## Supported Frameworks

| Framework | Supported |
| --------- | --------- |
| .NET 9    | ✅         |
| .NET 10   | ✅         |

## Supported Features

| Category                       | Status |
| ------------------------------ | ------ |
| Request/Response               | ✅      |
| Event Publishing               | ✅      |
| Multiple Event Handlers        | ✅      |
| Assembly Discovery             | ✅      |
| Dependency Injection           | ✅      |
| Multiple Assembly Registration | ✅      |
| Multiple Registration Calls    | ✅      |
| Cancellation Tokens            | ✅      |
| Automated Tests                | ✅      |
| Benchmarks                     | ✅      |
| ECommerce Example              | ✅      |

---

# Documentation

The repository includes multiple resources for learning and validating MiniBus.

| Resource          | Description                           |
| ----------------- | ------------------------------------- |
| README            | Project overview and getting started  |
| ECommerce Example | Executable request and event workflow |
| Tests             | Automated behavior validation         |
| Benchmarks        | Dispatch performance evaluation       |
| CHANGELOG         | Release history                       |

---

# Support

If you encounter a bug, have a feature request or need clarification about the API, please open an issue in the GitHub repository.

Community feedback is always welcome and helps improve MiniBus.

---

# License

MiniBus is released under the MIT License.

See the LICENSE file for details.
