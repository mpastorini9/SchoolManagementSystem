# Dependency Injection

## Purpose

This document defines the dependency injection strategy used throughout the backend.

Dependency Injection (DI) promotes loose coupling between components by allowing dependencies to be provided externally rather than created internally.

The backend relies on Dependency Injection to improve maintainability, testability, and flexibility.

---

# Dependency Injection Philosophy

Components should depend on abstractions rather than concrete implementations.

Dependencies should be provided by the application's composition root instead of being manually instantiated.

This approach follows the Dependency Inversion Principle (DIP).

---

# Responsibilities

Dependency Injection is responsible for:

- Creating application services.
- Providing repository implementations.
- Resolving infrastructure services.
- Managing object lifetimes.
- Wiring application components together.

Dependency Injection should simplify component collaboration without hiding architectural boundaries.

---

# Dependency Registration

All application dependencies should be registered during application startup.

The composition root is the only place where concrete implementations should be associated with their abstractions.

Dependency registration should remain centralized and easy to understand.

---

# Constructor Injection

Constructor injection is the preferred method for providing dependencies.

Components should receive all required dependencies through their constructors.

This makes dependencies explicit and improves testability.

Constructor injection should be preferred over property injection or service location.

---

# Dependency Principle

A class should only depend on the services it truly needs.

If a constructor requires many dependencies, it may indicate that the class has accumulated too many responsibilities.

Large constructors should be treated as a design smell rather than solved by alternative injection techniques.

Reducing responsibilities is preferable to hiding dependencies.

---

# Service Lifetimes

Services should use the appropriate lifetime according to their responsibility.

Typical lifetimes include:

- Singleton
- Scoped
- Transient

The chosen lifetime should reflect the behavior and requirements of the component.

General guidelines include:

- **Singleton** for stateless services that can safely be shared for the lifetime of the application.
- **Scoped** for services that should exist once per request, such as business services or repositories.
- **Transient** for lightweight, stateless services that require a new instance every time they are requested.

The selected lifetime should always match the intended behavior of the component.

---

# Design Principles

Dependency Injection should follow these principles:

- Depend on abstractions.
- Keep dependencies explicit.
- Avoid hidden dependencies.
- Minimize coupling.
- Improve testability.
- Keep registration centralized.

---

# Anti-Patterns

The following practices should be avoided:

- Service Locator.
- Static service access.
- Manual dependency creation.
- Hidden dependencies.
- Excessive constructor parameters.

These practices increase coupling and make the application harder to maintain.

---

# Testing

Dependency Injection enables components to be tested independently.

Interfaces allow test doubles, mocks, or fakes to replace production implementations during testing.

This improves unit testing and reduces dependencies on infrastructure.

---

# Design Goals

The dependency injection strategy aims to provide:

- Loose coupling.
- Better maintainability.
- Easier testing.
- Flexible implementations.
- Clear dependencies.
- Consistent object creation.

Dependency Injection should make dependencies visible rather than hide architectural problems.

---

# Relationship with Other Documents

This document defines how dependencies are managed within the backend.

It should be read together with:

- LayerResponsibilities.md
- ApplicationServices.md
- ProjectStructure.md
- Configuration.md

Together, these documents define how backend components are created and connected.