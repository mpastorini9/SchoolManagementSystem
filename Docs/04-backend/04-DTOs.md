# Data Transfer Objects (DTOs)

## Purpose

This document defines the purpose, responsibilities, and design guidelines for Data Transfer Objects (DTOs).

DTOs are used to transfer data between architectural layers while keeping the Domain model isolated from presentation and infrastructure concerns.

They are not part of the business model.

---

# DTO Philosophy

A DTO represents the data required for a specific interaction.

It should contain only the information necessary for a request or response.

DTOs exist to simplify communication between layers and to prevent leaking internal implementation details.

---

# Responsibilities

DTOs are responsible for:

- Receiving request data.
- Returning response data.
- Defining API contracts.
- Isolating the Domain from external consumers.
- Reducing coupling between layers.

DTOs should be simple data containers.

---

# DTOs Should Not

DTOs should never:

- Contain business logic.
- Implement validation rules beyond structural constraints.
- Reference database concerns.
- Replace domain entities.
- Expose internal implementation details.

Their only responsibility is transporting data.

---

# Types of DTOs

The application may define different DTO types depending on their purpose.

Typical categories include:

- Request DTOs
- Response DTOs
- Summary DTOs
- Detail DTOs

Each DTO should serve a single use case.

---

# One DTO per Use Case

As a general rule, each business use case should define its own DTOs.

Avoid creating generic DTOs intended to support multiple operations.

Instead of reusing a single DTO across the application, prefer DTOs that explicitly represent the requirements of a specific interaction.

For example:

- CreateStudentRequest
- UpdateStudentRequest
- StudentResponse
- StudentSummaryResponse
- StudentDetailsResponse

It is preferable to have many small, purpose-specific DTOs than a few large, generic ones.

This approach improves:

- Readability
- Maintainability
- API evolution
- Separation of concerns
- Backward compatibility

Although it may increase the total number of DTO classes, each DTO remains small, focused, and easy to understand.

This follows the Single Responsibility Principle by ensuring that each DTO serves one specific purpose.

---

# Mapping

Application Services are responsible for mapping between:

- Request DTOs and Domain entities.
- Domain entities and Response DTOs.

Mapping should remain outside the Domain layer.

The Domain should never be aware of DTOs.

Automatic mapping libraries may be used when they improve readability and maintainability, but mapping should always remain explicit and understandable.

---

# Naming Conventions

DTO names should clearly describe their purpose.

Examples include:

- CreateStudentRequest
- UpdateStudentRequest
- StudentResponse
- AttendanceSummaryResponse

Names should communicate intent rather than implementation.

Request DTOs should end with **Request**.

Response DTOs should end with **Response**.

---

# Versioning

DTOs represent public contracts.

Changes that break compatibility should be carefully evaluated.

Whenever possible, existing contracts should remain stable.

---

# Design Principles

DTOs should be:

- Simple.
- Explicit.
- Focused on a single interaction.
- Easy to understand.
- Immutable when practical.
- Independent from the Domain model.

Large or generic DTOs should be avoided.

---

# Relationship with the Domain

Domain entities model the business.

DTOs model communication.

These are different responsibilities and should remain independent.

The Domain must never depend on DTOs.

---

# Relationship with Other Documents

This document defines how data is exchanged between layers.

It should be read together with:

- LayerResponsibilities.md
- Controllers.md
- ApplicationServices.md
- DomainEntities.md
- Validation.md

Together, these documents define how information flows through the backend.