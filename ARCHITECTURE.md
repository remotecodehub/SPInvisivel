# SP Invisível — Architecture Rules

## Solution boundaries

The solution follows a feature-oriented DDD architecture. Each feature is an aggregate boundary and its related types remain grouped inside that feature.

### `InvisibleSP.Domain`

- Contains domain modeling only: entities, value objects, enums, domain events, domain rules, and domain abstractions that are intrinsically domain concepts.
- Must not reference another solution project.
- Must not reference infrastructure, application, presentation, ASP.NET Core, Entity Framework Core, Identity, logging infrastructure, HTTP, or persistence concerns.
- `Common` is reserved for domain types and abstractions genuinely shared by multiple domain features. It must not become a dumping ground.

### `InvisibleSP.Application`

- Contains use-case orchestration and contracts: commands, queries, handlers, requests, responses, events, validators, pipeline behaviors/middleware abstractions, and application services.
- May depend on `InvisibleSP.Domain`.
- Must depend on abstractions rather than infrastructure implementations.
- Must not contain EF Core `DbContext`, repository implementations, Identity implementations, HTTP transport concerns, or UI concerns.

### `InvisibleSP.Infrastructure`

- Contains concrete implementations of application/domain abstractions.
- Owns persistence, `DbContext`, repositories, Identity infrastructure, localization infrastructure, external integrations, and other technical concerns.
- May depend on `InvisibleSP.Application` and `InvisibleSP.Domain`.
- Infrastructure implementations must not leak into domain models or application contracts.

### `InvisibleSP.Composition`

- Contains composition-root extensions for `WebApplicationBuilder` and `WebApplication`.
- Exposes one complete pre-build service-registration/setup method and one complete HTTP-pipeline setup method.
- Owns dependency-injection composition and middleware/endpoint mapping orchestration.
- May reference ASP.NET Core framework types and the projects required to compose the application.
- Application/domain code must never depend on `InvisibleSP.Composition`.

### `InvisibleSP`

- Is the presentation/entry-point project and contains the Blazor Interactive UI and controllers.
- Receives presentation requests and translates them into application commands/queries.
- Consumes application contracts and never accesses repositories or `DbContext` directly.
- Maps application results to presentation responses/view models where required.
- Startup composition must use `InvisibleSP.Composition` rather than duplicating service-registration or pipeline configuration.

## Dependency direction

The intended dependency direction is:

`InvisibleSP` → `InvisibleSP.Composition` → `InvisibleSP.Infrastructure` → `InvisibleSP.Application` → `InvisibleSP.Domain`

`InvisibleSP.Application` may also reference `InvisibleSP.Domain` directly. `InvisibleSP.Domain` remains dependency-free.

Infrastructure is an implementation detail. Presentation must not bypass the application layer to access infrastructure services, repositories, or persistence.

## Feature organization

Each project is organized by feature first, not by technical type globally.

Preferred:

```text
InvisibleSP.Application/
  Identity/
    Abstractions/
    Commands/
    Handlers/
    Queries/
    Requests/
    Responses/
    Validators/
```

Avoid structures such as a single solution-wide `Services/`, `Repositories/`, `Handlers/`, or `Models/` directory that mixes unrelated features.

The feature name should represent the aggregate/use-case boundary. `Common` is allowed only when a type is truly shared by more than one feature.

## C# source conventions

- Target .NET 10.
- Enable nullable reference types.
- Use file-scoped namespaces in every C# source file.
- Do not use `using` directives in individual source files.
- Every C# project must contain a root-level `GlobalUsings.cs`, at the same directory level as its `.csproj`.
- `GlobalUsings.cs` is the single location for project-wide imports.
- Keep global imports minimal: only namespaces required by multiple source files belong there.
- Use explicit `using` aliases in `GlobalUsings.cs` when namespaces expose ambiguous type names.
- Prefer modern C#/.NET 10 language and BCL APIs when they improve correctness, clarity, performance, or maintainability.
- Prefer async APIs for I/O-bound operations and propagate `CancellationToken` through application and infrastructure boundaries.
- Avoid service-locator patterns and direct `IServiceProvider` resolution in application/domain code.
- Keep public APIs documented when they are part of a reusable abstraction or library boundary.
- Do not introduce nullable suppression (`!`) without a concrete invariant that makes the value safe.

## Dependency injection

- Register dependencies in `InvisibleSP.Composition`.
- Prefer constructor injection.
- Register abstractions against infrastructure implementations.
- Keep lifetimes intentional: singleton only for stateless/thread-safe shared services, scoped for request/unit-of-work services, transient for lightweight stateless services where appropriate.
- Do not register infrastructure types directly in presentation components/controllers when an application abstraction exists.

## Application flow

The normal request flow is:

`Blazor page/controller request` → `Command/Query` → `Handler` → `Application abstraction` → `Infrastructure implementation` → `Repository` → `DbContext` → `Entity/result` → `Application response` → `Presentation`

A handler should orchestrate a use case rather than become a persistence abstraction. Mapping between domain entities and application responses belongs at the application boundary unless the mapping is itself a domain concern.

## Domain rules

- Domain entities enforce their own invariants whenever practical.
- Domain models must not depend on UI, persistence, HTTP, serialization, or framework-specific infrastructure.
- Avoid anemic domain models when business invariants can naturally be expressed in the domain layer.
- Domain events represent meaningful domain occurrences; application/infrastructure concerns subscribe through appropriate abstractions.

## Global build enforcement

`Directory.Build.props` establishes the shared .NET 10 compiler/analyzer defaults for all projects beneath the repository root.

`Directory.Build.targets` validates that every C# project has a root-level `GlobalUsings.cs` before compilation.

The repository `.editorconfig` enforces file-scoped namespaces and centralized import conventions. Build-time analyzers are enabled so code-style and analyzer feedback is visible during builds.
