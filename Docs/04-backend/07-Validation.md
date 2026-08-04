# Validation

## Purpose

This document defines the validation strategy for the backend.

Validation ensures that incoming data is structurally correct, business rules are respected, and invalid operations are rejected before affecting the system.

Validation should occur at the appropriate architectural layer according to its responsibility.

---

# Validation Philosophy

Not all validations are the same.

Different kinds of validation belong to different layers of the application.

Keeping validation close to its responsibility improves maintainability and reduces duplication.

---

# Validation Principle

Validation should occur as early as possible, but no earlier.

Each validation belongs to the layer that has enough information and responsibility to perform it correctly.

Examples include:

- Malformed or incomplete requests should be rejected by the API layer.
- Business rule validation should be handled by the Application and Domain layers.
- Database constraints should be enforced by the Infrastructure layer.

Performing validation too early can duplicate business logic.

Performing validation too late can waste resources and produce less meaningful errors.

Every validation should exist in exactly one place, according to its responsibility.

---

# Types of Validation

Validation can be divided into three categories:

- Structural Validation
- Business Validation
- Persistence Validation

Each category should be handled by the appropriate layer.

---

# Structural Validation

Structural validation verifies that incoming data has the correct format.

Examples include:

- Required fields
- Maximum length
- Minimum length
- Data types
- Valid date formats
- Email format
- Numeric ranges

Structural validation belongs to the API layer.

Its purpose is to reject malformed requests before they reach the business logic.

---

# Business Validation

Business validation verifies that business rules are respected.

Examples include:

- A student cannot be enrolled twice in the same course.
- Attendance cannot be registered twice for the same student and date.
- A course cannot exceed its maximum capacity.
- A teacher must exist before being assigned to a course.

Business validation belongs to the Application and Domain layers.

These validations enforce business consistency.

---

# Persistence Validation

Persistence validation handles constraints enforced by the persistence mechanism.

Examples include:

- Unique indexes
- Foreign key constraints
- Concurrency conflicts

Persistence validation belongs to the Infrastructure layer.

The business should not depend on database-specific validations.

---

# Validation Principles

Validation should be:

- Explicit.
- Predictable.
- Consistent.
- Reusable when appropriate.
- Easy to understand.

Validation should never be hidden inside unrelated components.

Validation should protect the system, not complicate it.

---

# Validation Flow

A typical request is validated in the following order:

1. Structural validation.
2. Business validation.
3. Persistence validation.

Rejecting invalid requests early reduces unnecessary processing.

---

# Error Messages

Validation errors should:

- Clearly describe the problem.
- Identify the affected field when applicable.
- Avoid exposing internal implementation details.
- Be suitable for API consumers.

Error messages should help clients correct the request.

---

# Design Goals

The validation strategy aims to provide:

- Reliable business behavior.
- Consistent API responses.
- Clear separation of responsibilities.
- Better maintainability.
- Improved user experience.

The objective is to ensure that every validation has a single, well-defined responsibility and location within the architecture.

---

# Relationship with Other Documents

This document defines the validation strategy of the backend.

It should be read together with:

- LayerResponsibilities.md
- Controllers.md
- ApplicationServices.md
- DTOs.md
- ExceptionHandling.md

Together, these documents define how invalid requests are detected and handled.