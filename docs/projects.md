# Project Guide

| Project | Path | Responsibility | XML documentation in Release |
| --- | --- | --- | --- |
| InvisibleSP.Domain | `src/InvisibleSP.Domain/InvisibleSP.Domain.csproj` | Domain model and domain abstractions | Yes |
| InvisibleSP.Application | `src/InvisibleSP.Application/InvisibleSP.Application.csproj` | Use cases, contracts, handlers, validation | Yes |
| InvisibleSP.Infrastructure | `src/InvisibleSP.Infrastructure/InvisibleSP.Infrastructure.csproj` | Persistence, Identity-like services, JWT, integrations | Yes |
| InvisibleSP.Composition | `src/InvisibleSP.Composition/InvisibleSP.Composition.csproj` | Dependency injection and application composition | Yes |
| InvisibleSP | `src/InvisibleSP/InvisibleSP.csproj` | Blazor Interactive Server presentation and HTTP controllers | Yes |
| InvisibleSP.UnitTests | `tests/InvisibleSP.UnitTests/InvisibleSP.UnitTests.csproj` | Unit and infrastructure-backed tests | No file publication |

## Project-specific rules

### Domain

Keep the project dependency-free from the other solution projects and infrastructure frameworks. Domain abstractions must express business concepts rather than persistence mechanics.

### Application

Keep use-case orchestration and contracts independent of infrastructure implementations. Requests, handlers, validators, and application abstractions belong here. Infrastructure types must not leak into application contracts.

### Infrastructure

Implement application and domain abstractions here. Persistence and Identity-like behavior are infrastructure concerns. Keep technical details out of application and domain models.

### Composition

Centralize dependency registration and HTTP pipeline configuration. The entry-point project should call these extensions instead of duplicating startup configuration.

### Presentation

Controllers and Blazor components translate presentation input into application requests. Do not access `DbContext`, repositories, or other infrastructure services directly when an application abstraction exists.

### Tests

Use the actual production implementation when the test validates production behavior. Test doubles are appropriate for handler delegation and isolated application orchestration tests. Test XML comments are required, but the shared Release documentation target excludes this project from publishing XML files.
