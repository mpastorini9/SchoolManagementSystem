# Domain Entities

## Purpose

This document defines the design principles, responsibilities, and implementation guidelines for domain entities.

Domain entities represent the core business concepts of the system and encapsulate the state and behavior required to maintain business consistency.

They are the foundation of the business domain and should remain independent of external technologies.

---

# Entity Philosophy

A domain entity represents a real business concept.

Examples include:

- Student
- Teacher
- Course
- Attendance
- Student Record

Entities should model the business, not the database.

---

# Responsibilities

Domain entities are responsible for:

- Representing business concepts.
- Maintaining valid internal state.
- Enforcing business invariants.
- Exposing meaningful business behavior.
- Protecting their own consistency.

Entities should be responsible for themselves rather than relying on external components whenever possible.

---

# Identity

Every entity has a unique identity.

The identity distinguishes one business object from another regardless of changes to its attributes.

Entity identity should remain stable throughout its lifetime.

---

# State Management

Entities are responsible for maintaining a valid state.

State changes should occur through meaningful business operations rather than unrestricted property modifications.

Business consistency should always be preserved.

---

# Business Behavior

Business behavior should live as close as possible to the entities it affects.

Examples include:

- Enroll a student.
- Change course assignment.
- Register attendance.
- Activate a teacher.

Entities should expose business operations instead of acting as passive data containers.

---

# Encapsulation

Entity state should be protected.

Consumers should interact with entities through well-defined methods instead of directly modifying internal data whenever possible.

Encapsulation improves consistency and reduces accidental misuse.

---

# Persistence Independence

Domain entities should not depend on:

- Entity Framework
- SQL Server
- HTTP
- JSON
- Controllers
- External services

Persistence is an infrastructure concern.

The domain should remain independent of implementation details.

---

# Relationships

Entities may reference other domain entities when required by the business model.

Relationships should represent real business concepts rather than database implementation details.

Ownership and lifecycle should remain explicit.

---

# Value Objects

Some business concepts may eventually be modeled as Value Objects.

Examples might include:

- Address
- Email
- Phone Number
- Money

Value Objects should be immutable whenever possible.

Their introduction should simplify the domain rather than increase complexity.

---

# Future Evolution

As the project grows, the domain model may incorporate additional concepts such as:

- Domain Events
- Aggregate Roots
- Value Objects
- Domain Services
- Specifications

These concepts should only be introduced when they provide clear business value.

---

# Design Goals

Domain entities should remain:

- Independent
- Expressive
- Cohesive
- Testable
- Maintainable
- Focused on business behavior

The domain model should evolve with the business rather than with technical frameworks.

---

# Relationship with Other Documents

This document defines how business entities are modeled.

It should be read together with:

- LayerResponsibilities.md
- ApplicationServices.md
- DTOs.md
- Validation.md
- DomainModel.md (07-data)
- EntityRelationships.md (07-data)

Together, these documents define the business model of the application.