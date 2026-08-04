# REST Conventions

## Purpose

This document defines the REST conventions adopted by the application's API.

Consistent conventions improve readability, predictability, and long-term maintainability, allowing consumers to understand new endpoints without additional documentation.

These conventions apply to every HTTP endpoint exposed by the system.

---

# REST Philosophy

REST is a communication style, not merely a collection of HTTP methods.

The API should expose business resources through clear, predictable, and standardized interactions.

Every endpoint should behave consistently with the rest of the API.

---

# Resource-Oriented Design

Endpoints should represent business resources rather than technical operations.

Examples of resources include:

- Students
- Courses
- Teachers
- Attendances
- Student Records

Resources should use plural nouns.

---

# URI Design

URIs should be:

- Simple.
- Predictable.
- Stable.
- Hierarchical when appropriate.

URIs identify resources.

They should not describe implementation details or execution logic.

Examples:

```
GET    /students
GET    /students/{id}
POST   /students
PUT    /students/{id}
DELETE /students/{id}
```

---

# HTTP Methods

HTTP methods should reflect the intended operation.

| Method | Purpose |
|---------|----------|
| GET | Retrieve resources |
| POST | Create new resources |
| PUT | Replace existing resources |
| PATCH | Partially update resources |
| DELETE | Remove resources |

Method semantics should remain consistent throughout the API.

---

# Resource Naming

Resource names should:

- Use plural nouns.
- Use lowercase letters.
- Separate words with hyphens when necessary.
- Avoid verbs.

Good examples:

```
/students
/student-records
/teachers
```

Avoid:

```
/getStudents
/createStudent
/deleteCourse
```

The HTTP method already communicates the action.

---

# Nested Resources

Nested resources may be used when they clearly represent ownership or hierarchy.

Examples:

```
/courses/{id}/students
/students/{id}/records
```

Nested resources should remain shallow whenever possible.

Deep nesting reduces readability and flexibility.

---

# Stateless Communication

Every request should contain all information necessary for processing.

The server should not rely on previous requests or session state to interpret the current request.

Stateless communication improves scalability and reliability.

---

# Response Consistency

Responses should follow consistent structures across the API.

Consumers should be able to predict:

- Success responses.
- Validation failures.
- Error responses.
- Resource representations.

Consistency reduces integration complexity.

---

# Idempotency

Operations should respect HTTP semantics.

For example:

- GET should not modify data.
- PUT should be idempotent.
- DELETE should remain safe to repeat.
- POST may create new resources.

Consumers should be able to rely on predictable behavior.

---

# Future Evolution

REST conventions should remain stable over time.

New endpoints should extend existing patterns rather than introducing new conventions.

Consistency is more valuable than individual optimization.

---

# Design Principles

REST conventions follow these principles:

- Resource-oriented design.
- Predictable behavior.
- Standard HTTP semantics.
- Simplicity.
- Consistency.
- Stability.

Well-designed conventions reduce the need for documentation.

---

# Relationship with Other Documents

This document defines the REST conventions used throughout the API.

It should be read together with:

- APIOverview.md
- Endpoints.md
- RequestResponse.md
- Versioning.md
- ErrorResponses.md

Together, these documents define how the API is designed and consumed.