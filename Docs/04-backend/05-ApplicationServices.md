# Application Services

## Purpose

This document defines the responsibilities, organization, and design principles of the Application Services layer.

Application Services coordinate business use cases by orchestrating domain logic, infrastructure, and external dependencies while keeping the business workflow clear and maintainable.

They represent the entry point for business operations after a request leaves the API layer.

---

# Responsibilities

Application Services are responsible for:

- Executing business use cases.
- Coordinating domain entities.
- Managing application workflows.
- Calling repositories.
- Invoking external services.
- Managing transactions.
- Returning DTOs.
- Coordinating validation.

Application Services orchestrate business operations but should avoid containing complex domain rules.

---

# Design Principles

Application Services should follow these principles:

- One service per business use case.
- Single Responsibility Principle.
- Explicit dependencies.
- High cohesion.
- Low coupling.
- Readable business workflows.

Services should express business intent rather than technical implementation.

---

# Service Responsibilities

Application Services may:

- Load domain entities.
- Execute business workflows.
- Call repositories.
- Invoke infrastructure services.
- Coordinate multiple domain entities.
- Return application DTOs.

Application Services should not become repositories of business knowledge.

---

# What Application Services Should Not Do

Application Services should not:

- Handle HTTP requests.
- Build HTTP responses.
- Access the database directly.
- Contain presentation logic.
- Replace domain entities.
- Duplicate business rules already enforced by the domain.

These responsibilities belong to other architectural layers.

---

# Service Granularity

Services should remain focused.

Prefer small services that represent a single business operation.

Examples include:

- CreateStudent
- UpdateStudent
- RegisterAttendance
- GenerateReport
- RegisterPayment

Avoid large services responsible for unrelated operations.

---

# Dependency Management

Application Services depend only on abstractions.

Dependencies should be injected through constructors.

Application Services should never instantiate infrastructure implementations directly.

---

# Transactions

Application Services are responsible for coordinating transactional operations when multiple business actions must succeed together.

Transaction management should remain explicit and predictable.

---

# Validation

Application Services coordinate validation before executing business operations.

Validation may include:

- Request validation.
- Business validation.
- Permission validation.
- Existence checks.

Detailed validation rules are described in Validation.md.

---

# Return Types

Application Services should return application DTOs.

Domain entities should remain inside the business layer whenever possible.

Services should expose only the information required by the caller.

---

# Error Handling

Application Services should throw meaningful business exceptions when business rules cannot be satisfied.

Technical exceptions should not leak into higher layers without appropriate handling.

Detailed exception rules are described in ExceptionHandling.md.

---

# Future Organization

As the project grows, Application Services should be organized by feature instead of technical type.

Example:

Application/

- Students/
    - CreateStudent/
    - UpdateStudent/
    - DeleteStudent/

- Attendance/
    - RegisterAttendance/
    - GetAttendance/

- Billing/
    - RegisterPayment/

This organization improves discoverability, scalability, and long-term maintainability.

---

# Relationship with Other Documents

This document defines how business use cases are implemented.

It should be read together with:

- LayerResponsibilities.md
- Controllers.md
- DTOs.md
- DomainEntities.md
- Validation.md
- ExceptionHandling.md

Together, these documents define the behavior of the Application layer.
