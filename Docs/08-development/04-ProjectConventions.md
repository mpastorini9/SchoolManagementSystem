# Project Conventions

## Purpose

This document defines the project-specific conventions followed throughout the School Management System.

Its purpose is to promote consistency across the codebase by documenting common decisions that are not tied to a specific programming language or framework.

These conventions should be followed unless there is a clear reason to do otherwise.

---

# Consistency

New code should follow the existing conventions of the project.

Maintaining consistency across the codebase is more valuable than introducing personal preferences or alternative styles.

---

# Project Organization

Files, folders, and project structure should remain organized and predictable.

New artifacts should be placed in the most appropriate location according to the project's architecture.

Avoid creating new structures when an existing one already serves the same purpose.

---

# Naming Conventions

Names should be consistent throughout the project.

When introducing new concepts:

- Reuse existing terminology whenever possible.
- Avoid synonyms for the same concept.
- Use business language rather than technical jargon whenever appropriate.

A concept should have a single, consistent name across the entire project.

---

# Reusing Existing Solutions

Before introducing new patterns or implementations, contributors should first evaluate whether an existing solution already addresses the same problem.

Consistency is generally preferred over introducing multiple ways to solve similar problems.

---

# Incremental Evolution

The project should evolve through small, incremental improvements.

Large refactorings should only be introduced when they provide clear long-term value and can be implemented safely.

---

# Backward Compatibility

Whenever practical, new changes should minimize disruption to existing functionality.

Breaking changes should be introduced intentionally and with a clear justification.

---

# Decision Making

Engineering decisions should align with the project's architectural principles and long-term vision.

Short-term convenience should not compromise long-term maintainability.

---

# Relationship with Other Documents

This document defines project-wide engineering conventions.

Related engineering practices are documented in:

- Development Workflow
- Coding Standards
- Git Workflow
- Code Review