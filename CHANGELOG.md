# Changelog

All notable changes to MiniBus will be documented in this file.

The format is inspired by Keep a Changelog, and this project follows Semantic Versioning.

## [1.0.0] - 2026-09-21

### Added

* Initial stable release of MiniBus.
* Request/response messaging through `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`.
* Event publishing through `IEventHandler<TEvent>`.
* `IMiniBus` abstraction with `Send` and `Publish` operations.
* Assembly-based handler discovery.
* Dependency Injection integration.
* Multiple event handlers for a single event.
* Handler registration across multiple assemblies.
* Support for multiple `AddMiniBus` registrations.
* Idempotent assembly registration.
* Cancellation token propagation across request and event dispatch.
* .NET 9 and .NET 10 support.
* ECommerce example application.
* Benchmark project.

### Changed

* Organized the MiniBus architecture around messaging abstractions, handler discovery, dispatching and dependency injection.
* Introduced dedicated request and event handler wrappers for runtime dispatch.
* Simplified `MiniBus` to focus exclusively on request and event dispatch.
* Removed unsafe request response casting from the dispatch pipeline.
* Organized Dependency Injection extensions under the `TinyBlueWhale.MiniBus.DependencyInjection` namespace.

### Validation

* Validated request dispatch and response handling.
* Validated event publishing with multiple handlers.
* Validated sequential event handler execution.
* Validated handler discovery across multiple assemblies.
* Validated duplicate request handler detection.
* Validated repeated and multiple `AddMiniBus` registrations.
* Validated MiniBus and handler Dependency Injection lifetimes.
* Validated missing request handler behavior.
* Validated cancellation token propagation.
* Validated null and invalid registration arguments.
* Verified the MiniBus v1 architecture across .NET 9 and .NET 10.

### Notes

* MiniBus is an in-process messaging library.
* Each request type must have exactly one registered request handler.
* Events may have zero or multiple registered handlers.
* Event handlers are executed sequentially in registration order.
* MiniBus does not provide cross-process messaging, persistence or message transport.
