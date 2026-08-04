# Project Context

## Purpose

This document describes the current context of the School Management System project.

It provides the background necessary to understand the project's current stage, development priorities, technology choices, and immediate objectives.

Unlike the Vision, which defines the desired future state, this document reflects the project's present reality and is expected to evolve over time.

---

# Project Overview

School Management System is a Software as a Service (SaaS) platform being developed for educational institutions.

Its long-term objective is to provide a unified solution for managing students, courses, attendance, academic records, teachers, parents, payments, documents, notifications, and other administrative processes.

This is not a learning exercise or tutorial project.

It is intended to become a production-ready software product capable of supporting real educational institutions.

---

# Current Development Stage

The project is currently focused on delivering its first Minimum Viable Product (MVP).

At this stage, all development efforts are concentrated on completing the Attendance module before expanding the platform with additional business capabilities.

The MVP serves as the foundation for validating the product, its architecture, and its development practices in a real-world environment.

---

# Current Priorities

The project currently prioritizes:

- Completing the Attendance MVP.
- Delivering production-ready functionality rather than incomplete features.
- Building a stable architectural foundation.
- Maintaining a clean and well-documented codebase.
- Validating the product in real educational environments.

Every development decision should support these priorities.

---

# Development Philosophy

The project follows a simple development philosophy:

> Complete one module to production quality before starting the next.

The objective is not to maximize the number of implemented features.

Instead, the goal is to deliver reliable, maintainable, and valuable functionality through incremental evolution.

Current engineering priorities include:

- Simplicity over complexity.
- Readability over clever code.
- Maintainability over premature optimization.
- Incremental development.
- Business value over technical novelty.

---

# Technology Context

Current technologies include:

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- Swagger / OpenAPI
- InMemory Database (development only)

Planned technologies include:

- SQL Server
- Entity Framework Migrations

The project currently follows a monolithic architecture.

Additional architectural complexity will only be introduced when it provides measurable value.

---

# Current Architecture

The current implementation is intentionally simple and consists primarily of:

- Controllers
- Application DTOs
- Domain Models
- Entity Framework DbContext
- Swagger / OpenAPI
- Entity Framework Core

The architecture emphasizes clarity, maintainability, and rapid product evolution over premature optimization.

---

# Current Product Status

Implemented modules:

- Students
- Courses

Module under active development:

- Attendance

Future modules include:

- Student Records
- Teacher Management
- Parent Management
- Billing
- Notifications
- Document Management
- Administration

These capabilities are intentionally postponed until the Attendance module reaches production quality.

---

# Immediate Goal

The current milestone is the completion of Attendance MVP.

The module should allow teachers to:

- Select a course.
- Select a date.
- View all students in the selected course.
- Register absences efficiently.
- Generate attendance records automatically.
- Modify attendance when necessary.
- Review attendance history.

Once completed, the Attendance module should provide a stable foundation for frontend development without requiring significant API redesign.

---

# Design Principles

The current development stage is guided by the following principles:

- Deliver working software before expanding functionality.
- Prioritize product validation over feature quantity.
- Build sustainable foundations before scaling.
- Keep the architecture as simple as possible.
- Introduce complexity only when justified.
- Preserve consistency across the entire system.

Every short-term decision should contribute to the project's long-term vision.

---

# Relationship with Other Documents

This document describes the current state of the project.

It should be read together with:

- Vision.md
- Scope.md
- ProductRoadmap.md

The Vision defines the long-term direction.

The Scope defines the boundaries of the product.

The Project Context explains the project's current situation.

The Product Roadmap defines the planned evolution from the current state toward the long-term vision.