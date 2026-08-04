# AI Development Guidelines

## Purpose

This document defines how AI assistants should collaborate on the development of SchoolManagementSystem.

The goal is not only to generate working code, but also to preserve consistency, maintainability, and long-term project quality.

Any AI assistant working on this repository must follow these guidelines.

---

# Development Principles

Always prioritize:

- Simplicity
- Readability
- Maintainability
- Incremental development

Avoid unnecessary complexity.

The simplest correct solution is usually the preferred one.

---

# Current Objective

The only active development target is:

**Attendance V0.1**

Do not implement features outside this module unless explicitly requested.

Future modules already exist in the roadmap but are intentionally postponed.

---

# Development Workflow

Before writing code:

1. Understand the current implementation.
2. Analyze existing classes and relationships.
3. Identify missing functionality.
4. Explain the proposed solution.
5. Wait for approval before implementing major changes.

Do not rewrite large portions of the project unless explicitly requested.

---

# Code Style

Follow the existing project style.

Use:

- PascalCase for classes and properties.
- camelCase for local variables and parameters.
- Meaningful names.
- Small and readable methods.

Avoid abbreviations.

---

# Architecture

Current architecture is intentionally simple.

Current layers:

- Controllers
- DTOs
- Models
- DbContext

Do not introduce additional architectural layers unless requested.

Specifically, do NOT introduce:

- Clean Architecture
- Repository Pattern
- CQRS
- MediatR
- Microservices

These may be adopted in future versions if they provide real value.

---

# Business Logic

Business rules belong in the backend.

The frontend should remain as simple as possible.

Avoid duplicating business rules in multiple places.

---

# API Design

Endpoints should be:

- Consistent
- Predictable
- REST-oriented
- Easy to consume by a frontend application

Always use appropriate HTTP status codes.

Validate requests before processing.

Return meaningful error messages.

---

# Entity Framework

Prefer Entity Framework Core features over custom implementations.

Use navigation properties whenever appropriate.

Avoid unnecessary complexity.

---

# Validation

Always validate:

- Invalid identifiers
- Missing entities
- Duplicate operations
- Invalid requests

Never assume client input is correct.

---

# Documentation

Whenever a relevant architectural or business decision is made, explain:

- Why the change is necessary.
- What problem it solves.
- Possible alternatives.
- Long-term impact.

The objective is to make the project understandable for future contributors.

---

# Collaboration Rules

Large implementations should be divided into small iterations.

Each iteration should include:

- Goal
- Files to modify
- Implementation
- Testing instructions

Do not continue automatically to the next iteration without user approval.

---

# Long-Term Vision

Every implementation should contribute to building a real school management platform.

Code should not only work.

It should remain understandable, maintainable and extensible as the project grows.

Always prefer long-term quality over short-term convenience.