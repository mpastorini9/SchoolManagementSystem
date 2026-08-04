# Glossary

## Purpose

This document defines the official terminology used throughout the School Management System project.

Its purpose is to establish a shared vocabulary for developers, architects, QA engineers, product managers, and future contributors.

Consistent terminology improves communication, reduces ambiguity, and ensures that documentation, source code, and product discussions refer to concepts using the same language.

Whenever possible, each concept should have a single official definition.

---

# Business Terms

## School

An educational institution that uses the platform.

The platform is designed to support multiple schools through a multi-tenant architecture.

---

## Student

A person enrolled in an educational institution.

Students participate in one or more academic activities managed by the platform.

---

## Teacher

A person responsible for teaching, supervising, or managing one or more academic groups.

---

## Course

An academic group to which students are assigned.

Courses serve as the foundation for attendance, academic records, scheduling, and other educational processes.

---

## Attendance

The daily record of a student's attendance status.

Attendance represents the first production-ready business module of the platform.

---

## Student Record

The historical record containing relevant information about a student beyond attendance.

Examples include observations, disciplinary actions, achievements, and administrative notes.

---

## Module

An independent functional capability within the platform.

Each module owns a specific business responsibility while remaining part of the overall system.

Examples include Attendance, Student Records, Billing, and Reports.

---

## User

A person who can authenticate and access the platform.

A user represents an authenticated identity and may be associated with one or more business roles such as teacher, administrator, or parent.

User management is independent from business entities such as Student or Teacher.

---

# Technical Terms

## Engineering Handbook

The complete engineering documentation contained within the `docs` directory.

It defines the project's engineering principles, architecture, development practices, and shared knowledge.

---

## Modular Monolith

A software architecture where the application is deployed as a single unit while being internally organized into independent modules with well-defined responsibilities.

---

## API

The public interface through which clients communicate with the backend.

The API represents a stable contract rather than an implementation.

---

## Entity

A domain object that represents a business concept with identity and lifecycle.

Entities model the business domain rather than the database structure.

---

## DTO (Data Transfer Object)

An object used to transfer information between application boundaries.

DTOs represent contracts and should not contain business behavior.

---

## Repository

A component responsible for retrieving and persisting entities.

Repositories abstract persistence concerns from business logic.

---

## Service

A component responsible for coordinating business operations that cannot naturally belong to a single entity.

---

## ADR (Architecture Decision Record)

A document that records an important architectural decision.

Each ADR describes the context, the selected solution, alternative approaches, and the consequences of the decision.

---

# Naming Conventions

The following conventions apply throughout the project:

- Backend code is written in English.
- Technical documentation is written in English.
- User-facing interface text is written in Spanish.
- Business terminology may remain in Spanish when translation reduces clarity.
- Every business concept should have one official name across the entire project.

---

# Maintaining the Glossary

The glossary should evolve together with the product.

Whenever a new business or technical concept becomes part of the Engineering Handbook, it should be defined here before being used elsewhere.

Existing definitions should be updated rather than duplicated across multiple documents.

The glossary serves as the authoritative reference for project terminology.

---

# Relationship with Other Documents

This document defines the official terminology used throughout the Engineering Handbook.

Every document should follow the definitions established here.

If a document introduces a new business or technical concept, that concept should first be defined in this glossary.