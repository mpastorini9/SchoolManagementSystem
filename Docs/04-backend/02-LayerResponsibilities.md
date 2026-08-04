# Layer Responsibilities

## Purpose

This document defines the responsibilities, boundaries, and interaction rules of each architectural layer within the backend.

Its purpose is to ensure that every component has a clear responsibility, minimizing coupling while maximizing maintainability and scalability.

Each layer exists to solve a specific problem and should not assume responsibilities that belong elsewhere.

---

# Architectural Philosophy

The backend follows a layered architecture where responsibilities are explicitly separated.

Each layer should:

- Have a single responsibility.
- Expose only what is necessary.
- Depend only on allowed layers.
- Avoid leaking implementation details.

Clear boundaries are more important than technical convenience.

---

# Layer Overview

The backend is divided into four primary layers:

```text
API
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

Each layer has a well-defined purpose and communicates only through approved dependencies.

---

# API Layer

## Responsibilities

The API layer is responsible for:

- Receiving HTTP requests.
- Validating request format.
- Authentication.
- Authorization.
- Calling Application Services.
- Returning HTTP responses.
- API documentation.

## The API Layer Should Not

- Contain business logic.
- Access the database directly.
- Execute business workflows.
- Implement domain rules.

Controllers should remain thin and predictable.

---

# Application Layer

## Responsibilities

The Application layer is responsible for:

- Executing business use cases.
- Coordinating business workflows.
- Calling repositories.
- Coordinating validation.
- Managing transactions.
- Returning DTOs.
- Orchestrating multiple domain entities.

Application Services coordinate business operations without becoming the business itself.

## The Application Layer Should Not

- Handle HTTP concerns.
- Access infrastructure directly without abstractions.
- Contain presentation logic.
- Duplicate domain rules.

---

# Domain Layer

## Responsibilities

The Domain layer is responsible for:

- Business entities.
- Domain models.
- Core business rules.
- Business invariants.
- Domain behavior.

The Domain represents the business independently of frameworks, databases, or external technologies.

## The Domain Layer Should Not

- Know about HTTP.
- Know about Entity Framework.
- Know about databases.
- Know about JSON.
- Know about APIs.
- Know about infrastructure.

The Domain should remain pure.

---

# Infrastructure Layer

## Responsibilities

The Infrastructure layer is responsible for:

- Database access.
- Entity Framework implementation.
- Repository implementations.
- External services.
- Email providers.
- File storage.
- Logging implementation.
- Third-party integrations.

Infrastructure exists to support the application, not to define business behavior.

## The Infrastructure Layer Should Not

- Own business rules.
- Define business workflows.
- Replace domain logic.
- Contain presentation logic.

---

# Dependency Rules

Dependencies always point toward the center of the architecture.

Allowed dependencies:

- API → Application
- Application → Domain
- Infrastructure → Application
- Infrastructure → Domain

Forbidden dependencies include:

- Domain → Infrastructure
- Domain → API
- Application → API
- API → Infrastructure (except during application startup and dependency registration)

Violating these rules increases coupling and reduces maintainability.

---

# Request Flow

A typical request follows this flow:

```text
HTTP Request
        ↓
Controller
        ↓
Application Service
        ↓
Domain
        ↓
Repository Interface
        ↓
Infrastructure
        ↓
Database

Database
        ↑
Infrastructure
        ↑
Application
        ↑
Controller
        ↑
HTTP Response
```

Each layer performs only the work assigned to it.

---

# Layer Communication

Layers communicate through well-defined contracts.

Whenever possible:

- Application depends on interfaces.
- Infrastructure implements interfaces.
- Domain remains independent.
- DTOs cross layer boundaries instead of domain entities.

This minimizes coupling between layers.

---

# Design Goals

The layered architecture aims to provide:

- Maintainability.
- Testability.
- Scalability.
- Predictable code organization.
- Clear responsibilities.
- Long-term flexibility.

Every implementation decision should reinforce these goals.

---

# Relationship with Other Documents

This document defines the responsibilities of each backend layer.

It should be read together with:

- ProjectStructure.md
- Controllers.md
- ApplicationServices.md
- DomainEntities.md
- DTOs.md
- Validation.md
- ExceptionHandling.md

Together, these documents define the architectural standards of the backend.