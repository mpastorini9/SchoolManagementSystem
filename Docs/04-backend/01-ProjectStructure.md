# Project Structure

## Purpose

This document defines the physical organization of the backend solution.

Its purpose is to establish a predictable project structure that promotes maintainability, scalability, and clear separation of responsibilities.

Every source file should have an obvious and appropriate location within the solution.

---

# Design Principles

The project structure follows these principles:

- Separation of Concerns
- High Cohesion
- Low Coupling
- Feature Independence
- Predictable Organization
- Long-Term Maintainability

Folders should reflect architectural responsibilities rather than technical convenience.

---

# Solution Structure

The backend solution is organized into independent projects.

```text
src/

├── SchoolManagementSystem.Api
├── SchoolManagementSystem.Application
├── SchoolManagementSystem.Domain
└── SchoolManagementSystem.Infrastructure
```

Each project represents a distinct architectural layer.

Dependencies between projects must always respect the architecture.

---

# Project Responsibilities

## Api

Responsible for:

- HTTP endpoints
- Controllers
- Request handling
- Authentication
- Authorization
- API configuration
- Dependency Injection bootstrap

The API project should never contain business logic.

---

## Application

Responsible for:

- Use cases
- Application services
- DTOs
- Business orchestration
- Validation coordination
- Communication between layers

The Application layer coordinates business operations but does not manage infrastructure concerns.

---

## Domain

Responsible for:

- Business entities
- Domain rules
- Domain models
- Business invariants
- Core business concepts

The Domain layer represents the business itself and should remain independent of external technologies.

---

## Infrastructure

Responsible for:

- Database access
- Entity Framework
- External services
- File storage
- Email providers
- Logging implementation
- Third-party integrations

Infrastructure implements technical concerns required by the application.

---

# Dependency Rules

Dependencies must always flow inward.

```text
Api
    ↓
Application
    ↓
Domain

Infrastructure
    ↓
Application
    ↓
Domain
```

The Domain project should never depend on any other project.

The Application project must not depend on the API project.

Infrastructure should implement abstractions defined by higher layers whenever possible.

---

# Folder Organization

Each project should organize files according to their responsibility.

Examples include:

- Controllers
- Services
- DTOs
- Entities
- Interfaces
- Configurations
- Persistence
- Mappings

Folder names should remain simple, descriptive, and consistent.

---

# Naming Conventions

Projects should follow consistent naming conventions.

Examples:

- SchoolManagementSystem.Api
- SchoolManagementSystem.Application
- SchoolManagementSystem.Domain
- SchoolManagementSystem.Infrastructure

Namespaces should mirror the folder structure whenever possible.

---

# Scalability

The project structure should support future growth without major refactoring.

New modules should integrate into the existing architecture rather than introducing parallel structures.

Additional projects should only be created when they introduce a clear architectural benefit.

---

# Future Evolution

Future versions of the solution may introduce additional projects such as:

- SchoolManagementSystem.Tests
- SchoolManagementSystem.IntegrationTests
- SchoolManagementSystem.BackgroundJobs
- SchoolManagementSystem.SharedKernel

New projects should only be added when justified by architectural needs.

---

# Relationship with Other Documents

This document defines the physical organization of the backend.

It should be read together with:

- README.md
- LayerResponsibilities.md
- Controllers.md
- ApplicationServices.md
- DomainEntities.md
- DependencyInjection.md

Together, these documents define how the backend is organized and developed.