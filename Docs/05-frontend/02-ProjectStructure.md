# Project Structure

## Purpose

This document defines the organization of the frontend project.

A consistent project structure improves readability, maintainability, onboarding, and long-term scalability.

The structure should make it easy for developers to understand where new code belongs.

---

# Organizational Philosophy

The project structure should reflect responsibilities rather than technologies.

Folders should represent concepts of the application instead of implementation details.

A developer should be able to locate any feature with minimal effort.

---

# High-Level Structure

A typical frontend structure may resemble:

```text
src/

assets/
components/
pages/
layouts/
services/
hooks/
contexts/
routes/
styles/
utils/
types/
```

The exact structure may evolve over time while preserving architectural principles.

---

# Folder Responsibilities

Each folder should have a single primary responsibility.

Typical responsibilities include:

| Folder | Responsibility |
|----------|----------------|
| assets | Static resources |
| components | Reusable UI components |
| pages | Application pages |
| layouts | Shared page layouts |
| services | API communication |
| hooks | Custom React hooks |
| contexts | Shared application state |
| routes | Route definitions |
| styles | Global styling |
| utils | Utility functions |
| types | Shared TypeScript types or interfaces |

No folder should accumulate unrelated responsibilities.

---

# Feature Organization

As the application grows, related files should remain close together.

Files belonging to the same feature should be organized to minimize navigation across the project.

The structure should prioritize cohesion over strict categorization.

---

# Naming Conventions

Names should be:

- Clear
- Consistent
- Descriptive
- Predictable

Avoid abbreviations unless they are universally understood.

Folder names should clearly communicate their purpose.

---

# Import Organization

Imports should remain organized and easy to read.

A typical order is:

1. External libraries
2. Internal modules
3. Shared utilities
4. Relative imports
5. Styles

Consistent import organization improves readability.

---

# File Size

Files should remain focused on a single responsibility.

When a file becomes difficult to understand, it should be divided into smaller units.

Large files are often indicators of excessive responsibilities.

---

# Scalability

The project structure should support future modules without requiring major reorganization.

New functionality should integrate naturally into the existing architecture.

Growth should increase the number of features, not architectural complexity.

---

# Design Principles

The project structure follows these principles:

- Single responsibility
- High cohesion
- Low coupling
- Predictability
- Simplicity
- Maintainability

Organization should reduce cognitive load for developers.

---

# Design Goals

This structure aims to provide:

- Easy navigation
- Consistent organization
- Faster onboarding
- Better maintainability
- Sustainable growth

The project should remain understandable regardless of its size.

---

# Relationship with Other Documents

This document defines how the frontend codebase is organized.

It should be read together with:

- FrontendArchitecture.md
- Components.md
- Pages.md
- Routing.md

Together, these documents define the structural organization of the frontend.