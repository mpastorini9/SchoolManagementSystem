# Controllers

## Purpose

This document defines the responsibilities, design principles, and implementation guidelines for API controllers.

Controllers are responsible for exposing the application's functionality through HTTP endpoints while delegating business operations to the Application layer.

Controllers should remain thin, predictable, and easy to understand.

---

# Responsibilities

Controllers are responsible for:

- Receiving HTTP requests.
- Validating request models.
- Calling Application Services.
- Returning HTTP responses.
- Managing HTTP status codes.
- Applying authorization attributes.
- Defining API routes.

Controllers act as the entry point into the backend.

---

# Controllers Should Not

Controllers should never:

- Contain business logic.
- Access the database directly.
- Execute business workflows.
- Build domain entities.
- Implement validation rules beyond request format validation.
- Communicate directly with Infrastructure.

Business behavior belongs to the Application and Domain layers.

---

# Controller Design Principles

Controllers should follow these principles:

- One responsibility per controller.
- Thin endpoints.
- Predictable behavior.
- Explicit dependencies.
- Consistent naming.
- RESTful conventions.

Controllers should primarily coordinate request and response handling.

---

# Endpoint Responsibilities

Each endpoint should:

1. Receive the request.
2. Validate the request model.
3. Call the appropriate Application Service.
4. Return the appropriate HTTP response.

The controller should perform no additional business processing.

---

# Routing

Routes should be:

- Simple.
- Consistent.
- Resource-oriented.
- Versionable.

Controllers should expose resources rather than implementation details.

---

# HTTP Status Codes

Controllers should return appropriate HTTP status codes.

Typical responses include:

- 200 OK
- 201 Created
- 204 No Content
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict
- 500 Internal Server Error

Status codes should accurately represent the outcome of the request.

---

# Dependency Injection

Controllers receive their dependencies through constructor injection.

Controllers should never instantiate services manually.

All dependencies should be registered by the application's dependency injection container.

---

# Error Handling

Controllers should rely on the application's global exception handling strategy.

Unexpected exceptions should not be handled individually unless a specific endpoint requires custom behavior.

Detailed exception handling is described in ExceptionHandling.md.

---

# Documentation

Every public endpoint should be documented through the API documentation system.

Endpoint documentation should clearly describe:

- Purpose
- Request
- Response
- Possible error responses

Documentation should remain synchronized with implementation.

---

# Relationship with Other Documents

This document defines the responsibilities of API controllers.

It should be read together with:

- LayerResponsibilities.md
- ApplicationServices.md
- DTOs.md
- Validation.md
- ExceptionHandling.md

Together, these documents define the behavior of the API layer.